#!/usr/bin/env python3
"""
ingest_all.py — full-coverage ingester for the eStudy backend.

Parses EVERY format in docs/Digital Assessments (xlsx, csv, xls, docx) into one
deduplicated question set, and the docs/PDF ... Manuals into a syllabus tree.
Emits JSON the Node seed.js loads into SQLite:

  scripts/ingest/out/questions.json   deduped questions (content_hash, source_format)
  scripts/ingest/out/syllabus.json    grades->subjects->units->topics->objectives
  scripts/ingest/out/_skipped.json     every file we couldn't parse, with reason
  scripts/ingest/out/_stats.json       summary counts

Dedup: questions seen in multiple formats collapse to one row by content_hash =
sha1(grade|subject|normalized_stem|sorted_options). Source priority xlsx>csv>xls>docx.

Usage: python scripts/ingest/ingest_all.py
"""
import sys, os, re, json, glob, csv, hashlib

ROOT_ASSESS = "docs/Digital Assessments"
ROOT_PDF = "docs/PDF Digital Content Mnauals Final"
OUT = os.path.join("scripts", "ingest", "out")

# ── helpers (shared with the original ingester) ──────────────────────
def norm(s): return ("" if s is None else str(s)).strip()
def norm_key(s): return re.sub(r"\s+", "", norm(s).lower())
def norm_stem(s): return re.sub(r"\s+", " ", norm(s).lower())

def grade_from_path(parts):
    # explicit "Grade N" wins over the band folder, so ECD-5/.../Grade 1 → "1"
    for p in parts:
        m = re.match(r"grade\s*0*([0-9]+)", p, re.I)
        if m: return m.group(1)
    for p in parts:
        if p.strip().upper() in ("ECD", "ECD-5"): return "ECD"
    return "?"

KNOWN_SUBJECTS = ["Biology","Chemistry","Physics","Mathematics","Math","English","Urdu",
                  "Science","Pak Studies","Pakistan Studies","General Knowledge","Computer",
                  "Islamiat","Islamiyat","Social Studies"]
SUBJECT_CANON = {"Math":"Mathematics","Pakistan Studies":"Pak Studies","Computer":"Computer Science",
                 "Islamiat":"Islamiyat"}
# filename subject codes: G09BiU01P01 -> Bi
CODE_SUBJECT = {"Bi":"Biology","Ch":"Chemistry","Ph":"Physics","M":"Mathematics","E":"English",
                "En":"English","U":"Urdu","S":"Science","Sc":"Science","GK":"General Knowledge",
                "PS":"Pak Studies","CS":"Computer Science","Is":"Islamiyat"}

def subject_from_path(parts):
    for p in parts:
        for s in KNOWN_SUBJECTS:
            if p.strip().lower() == s.lower():
                return SUBJECT_CANON.get(s, s)
    return None

def subject_from_filename(name):
    m = re.match(r"G\d{2}([A-Za-z]{1,2})U\d+", name)
    if m:
        return CODE_SUBJECT.get(m.group(1))
    return None

def unit_from_path(parts):
    for p in parts:
        if re.match(r"unit\s*", p, re.I): return p.strip()
    return ""

EMPTY = ("", "null", "-")

def content_hash(grade, subject, stem, options):
    key = f"{grade}|{subject}|{norm_stem(stem)}|{'|'.join(sorted(norm_stem(o) for o in options))}"
    return hashlib.sha1(key.encode("utf-8")).hexdigest()

def make_q(ctx, stem, options, correct, topic="", skill="", feedback="", fmt="xlsx"):
    subject = ctx["subject"]
    return {
        "grade": ctx["grade"], "subject": subject, "unit": ctx["unit"],
        "kind": "objective", "q": stem, "options": options, "correct": correct,
        "topic": topic or ctx["unit"] or subject, "skill": skill, "feedback": feedback,
        "paper": ctx["paper"], "source_format": fmt,
        "content_hash": content_hash(ctx["grade"], subject, stem, options),
    }

# ── row-based parsing (xlsx / csv / xls share the "std" layout) ──────
def parse_std_rows(rows, ctx, fmt):
    """rows: list of dicts keyed by normalized header."""
    out = []
    for r in rows:
        q = norm(r.get("question") or r.get("questions"))
        if not q: continue
        options, correct = [], -1
        for k in range(1, 7):
            opt = norm(r.get(f"option{k}"))
            if not opt: continue
            if norm(r.get(f"correct{k}")).upper().startswith("Y"):
                correct = len(options)
            options.append(opt)
        if len(options) < 2: continue
        if correct < 0: correct = 0
        out.append(make_q(ctx, q, options, correct,
                          topic=norm(r.get("topic")), skill=norm(r.get("skill") or r.get("skills")),
                          feedback=norm(r.get("feedback") or r.get("remarks")), fmt=fmt))
    return out

def rows_from_matrix(matrix):
    """Find a header row containing question+option1, return list of dict rows."""
    for hi, row in enumerate(matrix[:14]):
        keys = [norm_key(c) for c in row]
        kset = set(keys)
        if ("question" in kset or "questions" in kset) and any(re.fullmatch(r"option\d+", k) for k in keys):
            cols = {k: i for i, k in enumerate(keys) if k}
            out = []
            for row in matrix[hi + 1:]:
                d = {k: (row[i] if i < len(row) else "") for k, i in cols.items()}
                out.append(d)
            return out
    return None

# ── XLSX (openpyxl) ──────────────────────────────────────────────────
def parse_xlsx(path, ctx):
    import openpyxl
    wb = openpyxl.load_workbook(path, data_only=True, read_only=True)
    out = []
    for ws in wb.worksheets:
        matrix = [[norm(c) for c in row] for row in ws.iter_rows(values_only=True)]
        rows = rows_from_matrix(matrix)
        if rows:
            out += parse_std_rows(rows, ctx, "xlsx")
    return out

# ── CSV ───────────────────────────────────────────────────────────────
def parse_csv(path, ctx):
    with open(path, "r", encoding="utf-8-sig", errors="replace", newline="") as f:
        matrix = [row for row in csv.reader(f)]
    rows = rows_from_matrix(matrix)
    return parse_std_rows(rows, ctx, "csv") if rows else []

# ── XLS (legacy) via pyexcel-xls ──────────────────────────────────────
def parse_xls(path, ctx):
    try:
        from pyexcel_xls import get_data
    except Exception:
        raise RuntimeError("pyexcel-xls unavailable")
    data = get_data(path)
    out = []
    for _sheet, matrix in data.items():
        matrix = [[norm(c) for c in row] for row in matrix]
        rows = rows_from_matrix(matrix)
        if rows:
            out += parse_std_rows(rows, ctx, "xls")
    return out

# ── DOCX (prose questions) ────────────────────────────────────────────
OPT_RE = re.compile(r"^\s*([A-Da-d])[\.\)]\s+(.*)$")
ANS_RE = re.compile(r"^\s*(?:ans(?:wer)?|correct)\s*[:\-]?\s*([A-Da-d])", re.I)

def parse_docx(path, ctx):
    import docx
    doc = docx.Document(path)
    lines = [norm(p.text) for p in doc.paragraphs if norm(p.text)]
    out, i = [], 0
    while i < len(lines):
        line = lines[i]
        # a question line is followed by A./B./C./D. options
        if line and not OPT_RE.match(line) and not ANS_RE.match(line):
            opts, j, letters = [], i + 1, []
            while j < len(lines):
                m = OPT_RE.match(lines[j])
                if not m: break
                letters.append(m.group(1).upper()); opts.append(m.group(2).strip()); j += 1
            if len(opts) >= 2:
                correct = 0
                if j < len(lines):
                    am = ANS_RE.match(lines[j])
                    if am:
                        c = am.group(1).upper()
                        if c in letters: correct = letters.index(c)
                        j += 1
                stem = re.sub(r"^\s*(?:Q\.?\s*\d+[\.\):]?|\d+[\.\)])\s*", "", line).strip()
                out.append(make_q(ctx, stem, opts, correct, fmt="docx"))
                i = j; continue
        i += 1
    return out

PARSERS = {".xlsx": parse_xlsx, ".csv": parse_csv, ".xls": parse_xls, ".docx": parse_docx}
PRIORITY = {"xlsx": 0, "csv": 1, "xls": 2, "docx": 3}

def ctx_for(path):
    rel = os.path.relpath(path, ROOT_ASSESS).replace("\\", "/")
    parts = rel.split("/")
    name = os.path.splitext(os.path.basename(path))[0]
    subj = subject_from_path(parts) or subject_from_filename(name) or "General"
    return {"grade": grade_from_path(parts), "subject": subj,
            "unit": unit_from_path(parts), "paper": name}, rel

# ── question ingest (all formats, dedup) ──────────────────────────────
def ingest_questions():
    files = []
    for ext in PARSERS:
        files += glob.glob(os.path.join(ROOT_ASSESS, "**", f"*{ext}"), recursive=True)
    files = sorted(set(files))
    print(f"[questions] {len(files)} files across {list(PARSERS)}")

    by_hash = {}     # hash -> question (highest priority kept)
    skipped, stats = [], {"by_format": {}, "files": len(files), "parsed": 0}
    for n, path in enumerate(files):
        ext = os.path.splitext(path)[1].lower()
        ctx, rel = ctx_for(path)
        try:
            items = PARSERS[ext](path, ctx)
        except Exception as e:
            skipped.append({"file": rel, "format": ext, "reason": str(e)[:160]}); continue
        if not items:
            skipped.append({"file": rel, "format": ext, "reason": "no-questions"}); continue
        stats["parsed"] += 1
        stats["by_format"][ext] = stats["by_format"].get(ext, 0) + len(items)
        for it in items:
            h = it["content_hash"]
            prev = by_hash.get(h)
            if prev is None or PRIORITY[it["source_format"]] < PRIORITY[prev["source_format"]]:
                # keep richer record's feedback/topic if the new one lacks it
                if prev:
                    it["feedback"] = it["feedback"] or prev["feedback"]
                    it["skill"] = it["skill"] or prev["skill"]
                by_hash[h] = it
        if (n + 1) % 400 == 0:
            print(f"  ...{n+1}/{len(files)} files, {len(by_hash)} unique questions")

    questions = list(by_hash.values())
    stats["unique_questions"] = len(questions)
    stats["raw_questions"] = sum(stats["by_format"].values())
    stats["skipped_count"] = len(skipped)
    print(f"[questions] {stats['raw_questions']} raw -> {len(questions)} unique "
          f"(deduped {stats['raw_questions'] - len(questions)}), skipped {len(skipped)}")
    return questions, skipped, stats

# ── syllabus: baseline units from questions + objectives from PDFs ────
def syllabus_from_questions(questions):
    """Guaranteed structure: every (grade,subject) gets its units from the data."""
    seen = {}  # (grade,subject) -> {unit: set(topics)}
    for q in questions:
        key = (q["grade"], q["subject"])
        unit = q["unit"] or "Unit 1"
        seen.setdefault(key, {}).setdefault(unit, set())
        if q["topic"] and q["topic"] != unit:
            seen[key][unit].add(q["topic"])
    rows = []
    for (grade, subject), units in seen.items():
        for i, (uname, topics) in enumerate(sorted(units.items())):
            rows.append({
                "grade_code": grade, "subject": subject, "name": uname, "number": i + 1,
                "sort_order": i, "description": "", "source": "questions",
                "topics": [{"name": t, "objectives": []} for t in sorted(topics)[:40]],
                "objectives": [],
            })
    return rows

UNIT_HEAD = re.compile(r"^\s*(unit|chapter)\s+0*(\d+)\b[:\.\-]?\s*(.*)$", re.I)
OBJ_LINE = re.compile(r"^\s*(?:[•\-\*\d\.\)]+)\s*(.{8,})$")

def objectives_from_pdfs():
    """Best-effort: map each grade PDF -> {subject -> [objective lines]} by scanning
    for subject section headers and bullet/SLO lines. Resilient: returns {} on failure."""
    import pdfplumber
    out = {}  # grade -> {subject -> [objectives]}
    pdfs = sorted(glob.glob(os.path.join(ROOT_PDF, "*.pdf")))
    print(f"[syllabus] scanning {len(pdfs)} PDF manuals for objectives")
    subj_names = sorted(set(list(CODE_SUBJECT.values()) + ["Math", "General Knowledge"]), key=len, reverse=True)
    for path in pdfs:
        m = re.search(r"Grade\s*0*([0-9]+|ECD)", os.path.basename(path), re.I)
        grade = "ECD" if (m and m.group(1).upper() == "ECD") else (m.group(1) if m else "?")
        if "ECD" in os.path.basename(path).upper(): grade = "ECD"
        try:
            text = []
            with pdfplumber.open(path) as pdf:
                for page in pdf.pages:
                    text.append(page.extract_text() or "")
            lines = [norm(l) for l in "\n".join(text).split("\n") if norm(l)]
        except Exception as e:
            print(f"  ! {os.path.basename(path)}: {str(e)[:80]}")
            continue
        cur_subject = None
        gmap = out.setdefault(grade, {})
        for l in lines:
            # subject header?
            hit = next((s for s in subj_names if l.lower() == s.lower() or
                        re.match(rf"^{re.escape(s)}\b", l, re.I) and len(l) < len(s) + 14), None)
            if hit:
                cur_subject = SUBJECT_CANON.get(hit, hit); gmap.setdefault(cur_subject, [])
                continue
            if cur_subject and (l[:1] in "•-*" or OBJ_LINE.match(l)) and len(l) > 12:
                clean = OBJ_LINE.match(l)
                obj = (clean.group(1) if clean else l).strip()
                if 8 < len(obj) < 220 and not UNIT_HEAD.match(obj):
                    gmap[cur_subject].append(obj)
        # cap per subject
        for s in gmap: gmap[s] = gmap[s][:60]
        got = sum(len(v) for v in gmap.values())
        print(f"  {os.path.basename(path)} -> grade {grade}: {len(gmap)} subjects, {got} objective lines")
    return out

def build_syllabus(questions):
    rows = syllabus_from_questions(questions)
    try:
        pdf_obj = objectives_from_pdfs()
    except Exception as e:
        print(f"[syllabus] PDF pass failed ({str(e)[:80]}) — units-only syllabus")
        pdf_obj = {}
    # attach PDF objectives to the first unit of each (grade, subject)
    for row in rows:
        objs = pdf_obj.get(row["grade_code"], {}).get(row["subject"])
        if objs and row["number"] == 1:
            row["objectives"] = [{"text": o} for o in objs]
            row["source"] = "questions+pdf"
            row["ai_structured"] = 0
    return rows

def write(name, data):
    with open(os.path.join(OUT, name), "w", encoding="utf-8") as f:
        json.dump(data, f, ensure_ascii=False)

def main():
    os.makedirs(OUT, exist_ok=True)
    questions, skipped, stats = ingest_questions()
    # persist questions + skipped immediately so a later syllabus error can't lose the parse
    write("questions.json", questions)
    write("_skipped.json", {"count": len(skipped), "skipped": skipped})
    print(f"[write] questions.json ({len(questions)}) + _skipped.json ({len(skipped)})")

    syllabus = build_syllabus(questions)
    stats["syllabus_units"] = len(syllabus)
    write("syllabus.json", syllabus)
    write("_stats.json", stats)
    print(f"DONE: {len(questions)} questions, {len(syllabus)} syllabus units, "
          f"{len(skipped)} skipped. Output -> {OUT}")

if __name__ == "__main__":
    main()
