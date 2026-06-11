<template>
  <div class="lc-root">
    <AIHelper context="User is learning programming — Python, JavaScript, HTML, C++" :chips="['Explain this code', 'Show me an example', 'What is a loop?', 'Debug my code']" />

    <!-- Header -->
    <div class="lc-hero">
      <div class="lc-hero-icon">💻</div>
      <h1 class="lc-hero-title">Learn Coding</h1>
      <p class="lc-hero-sub">Beginner-friendly programming lessons · AI code helper · Practice exercises</p>
    </div>

    <!-- Language selector -->
    <div v-if="!activeLang" class="lc-lang-grid">
      <div v-for="lang in languages" :key="lang.id" @click="activeLang = lang.id" class="lc-lang-card">
        <div class="lc-lang-icon">{{ lang.icon }}</div>
        <div class="lc-lang-name">{{ lang.name }}</div>
        <div class="lc-lang-desc">{{ lang.desc }}</div>
        <div class="lc-lang-meta">{{ lang.lessons.length }} lessons</div>
      </div>
    </div>

    <!-- Language Lessons -->
    <div v-if="activeLang && !activeLesson" class="lc-lessons-view">
      <div class="lc-lessons-header">
        <button @click="activeLang = null" class="lc-back">← Languages</button>
        <h2 class="lc-lessons-title">{{ currentLang.icon }} {{ currentLang.name }}</h2>
      </div>
      <div class="lc-progress-bar"><div class="lc-progress-fill" :style="{width: progressPct + '%'}" /></div>
      <div class="lc-progress-label">{{ completedCount }}/{{ currentLang.lessons.length }} lessons completed</div>
      <div class="lc-lesson-list">
        <div v-for="(lesson, i) in currentLang.lessons" :key="lesson.id"
          @click="openLesson(lesson)"
          class="lc-lesson-item"
          :class="{ done: isCompleted(activeLang, lesson.id), locked: i > 0 && !isCompleted(activeLang, currentLang.lessons[i-1].id) && i !== 0 }">
          <div class="lc-lesson-num">{{ isCompleted(activeLang, lesson.id) ? '✓' : (i+1) }}</div>
          <div>
            <div class="lc-lesson-name">{{ lesson.title }}</div>
            <div class="lc-lesson-topic">{{ lesson.topic }}</div>
          </div>
          <div class="lc-lesson-badge" :class="lesson.difficulty">{{ lesson.difficulty }}</div>
        </div>
      </div>
    </div>

    <!-- Lesson Detail -->
    <div v-if="activeLesson" class="lc-lesson-detail">
      <div class="lc-detail-header">
        <button @click="activeLesson = null" class="lc-back">← Lessons</button>
        <h2 class="lc-detail-title">{{ activeLesson.title }}</h2>
      </div>

      <div v-for="(section, i) in activeLesson.sections" :key="i" class="lc-section">
        <div v-if="section.type === 'text'" class="lc-section-text">{{ section.content }}</div>
        <div v-else-if="section.type === 'code'" class="lc-section-code">
          <div class="lc-code-header">{{ section.lang || currentLang.name }} Code</div>
          <pre class="lc-code-block">{{ section.content }}</pre>
          <button @click="askAIAboutCode(section.content)" class="lc-code-ai-btn">🤖 AI Explain This</button>
        </div>
        <div v-else-if="section.type === 'output'" class="lc-section-output">
          <div class="lc-output-header">Output</div>
          <pre class="lc-output-block">{{ section.content }}</pre>
        </div>
        <div v-else-if="section.type === 'tip'" class="lc-section-tip">
          <span>💡</span> {{ section.content }}
        </div>
        <div v-else-if="section.type === 'quiz'" class="lc-quiz-block">
          <div class="lc-quiz-question">🧠 Quick Check: {{ section.question }}</div>
          <div class="lc-quiz-options">
            <button v-for="(opt, oi) in section.options" :key="oi"
              @click="checkAnswer(section, oi)"
              class="lc-quiz-opt"
              :class="{ correct: quizAnswered[i] === oi && oi === section.answer, wrong: quizAnswered[i] === oi && oi !== section.answer, right_show: quizAnswered[i] !== undefined && oi === section.answer }">
              {{ opt }}
            </button>
          </div>
          <div v-if="quizAnswered[i] !== undefined" class="lc-quiz-feedback" :class="{ ok: quizAnswered[i] === section.answer }">
            {{ quizAnswered[i] === section.answer ? '✅ Correct! ' + section.explanation : '❌ Wrong. ' + section.explanation }}
          </div>
        </div>
      </div>

      <!-- AI Code explainer panel -->
      <div v-if="codeExplanation" class="lc-code-exp">
        <div class="lc-code-exp-header">🤖 AI Code Explanation</div>
        <div class="lc-code-exp-text">{{ codeExplanation }}</div>
      </div>

      <button @click="markComplete" class="lc-complete-btn" :disabled="isCompleted(activeLang, activeLesson.id)">
        {{ isCompleted(activeLang, activeLesson.id) ? '✅ Completed!' : '✓ Mark as Complete' }}
      </button>
    </div>

    <PageFooter />
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import AIHelper from '@/components/platform/AIHelper.vue'
import PageFooter from '@/components/platform/PageFooter.vue'

const activeLang = ref(null)
const activeLesson = ref(null)
const quizAnswered = ref({})
const codeExplanation = ref('')
const aiLoadingCode = ref(false)

const completed = ref(JSON.parse(localStorage.getItem('bap_coding_progress') || '{}'))

function saveProgress() { localStorage.setItem('bap_coding_progress', JSON.stringify(completed.value)) }
function isCompleted(langId, lessonId) { return !!completed.value[langId + '_' + lessonId] }
function markComplete() {
  if (!activeLang.value || !activeLesson.value) return
  completed.value[activeLang.value + '_' + activeLesson.value.id] = true
  saveProgress()
}
function checkAnswer(section, idx) {
  const secIdx = activeLesson.value.sections.indexOf(section)
  quizAnswered.value = { ...quizAnswered.value, [secIdx]: idx }
}
function openLesson(lesson) { activeLesson.value = lesson; quizAnswered.value = {}; codeExplanation.value = '' }

async function askAIAboutCode(code) {
  aiLoadingCode.value = true; codeExplanation.value = ''
  try {
    const key = import.meta.env.VITE_GEMINI_API_KEY
    if (!key) throw new Error()
    const res = await fetch(`https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key=${key}`, {
      method:'POST', headers:{'Content-Type':'application/json'},
      body: JSON.stringify({ contents:[{ parts:[{ text:`Explain this code to a beginner Pakistani student (Grade 9-12) who is just learning programming. Explain line by line in simple English. Be encouraging and friendly:\n\n\`\`\`\n${code}\n\`\`\`` }] }] })
    })
    const data = await res.json()
    codeExplanation.value = data.candidates?.[0]?.content?.parts?.[0]?.text || 'No response.'
  } catch { codeExplanation.value = 'AI unavailable. Check your VITE_GEMINI_API_KEY.' }
  aiLoadingCode.value = false
}

const currentLang = computed(() => languages.find(l => l.id === activeLang.value))
const completedCount = computed(() => currentLang.value?.lessons.filter(l => isCompleted(activeLang.value, l.id)).length || 0)
const progressPct = computed(() => currentLang.value ? Math.round((completedCount.value / currentLang.value.lessons.length) * 100) : 0)

const languages = [
  {
    id: 'python', name: 'Python', icon: '🐍', desc: 'Best first language for beginners', lessons: [
      { id:'py1', title:'Introduction to Python', topic:'What is programming?', difficulty:'beginner', sections:[
        { type:'text', content:'Python is a powerful, easy-to-read programming language. It is used in web development, data science, AI, and automation. Python is one of the most popular languages in the world and is great for beginners because its syntax is clean and readable.' },
        { type:'tip', content:'Python uses indentation (spaces) instead of curly braces {}. This makes code very readable.' },
        { type:'code', lang:'Python', content:'# This is your first Python program\nprint("Assalamualaikum!")\nprint("Welcome to Python programming!")\nname = "Ahmed"\nprint("My name is", name)' },
        { type:'output', content:'Assalamualaikum!\nWelcome to Python programming!\nMy name is Ahmed' },
        { type:'quiz', question:'What does print() do in Python?', options:['Prints to a printer','Displays output on screen','Deletes a variable','None of the above'], answer:1, explanation:'print() displays text or values on the screen/console.' }
      ]},
      { id:'py2', title:'Variables and Data Types', topic:'Storing information', difficulty:'beginner', sections:[
        { type:'text', content:'Variables are like boxes that store information. In Python, you don\'t need to declare the type — Python figures it out automatically. The main data types are: int (whole numbers), float (decimal numbers), str (text), and bool (True/False).' },
        { type:'code', content:'# Variables\nname = "Fatima"       # str (string/text)\nage = 17              # int (integer)\ngpa = 3.85            # float (decimal)\nis_student = True     # bool (True/False)\n\nprint("Name:", name)\nprint("Age:", age)\nprint("GPA:", gpa)\nprint("Is student:", is_student)\nprint("Type of age:", type(age))' },
        { type:'output', content:'Name: Fatima\nAge: 17\nGPA: 3.85\nIs student: True\nType of age: <class \'int\'>' },
        { type:'quiz', question:'What type is the value 3.14 in Python?', options:['int','str','float','bool'], answer:2, explanation:'3.14 is a float (floating point) because it has a decimal point.' }
      ]},
      { id:'py3', title:'If-Else Conditions', topic:'Decision making', difficulty:'beginner', sections:[
        { type:'text', content:'Conditions allow your program to make decisions. Using if-elif-else, you can run different code depending on whether something is True or False.' },
        { type:'code', content:'marks = 75\n\nif marks >= 80:\n    grade = "A"\nelif marks >= 60:\n    grade = "B"\nelif marks >= 40:\n    grade = "C"\nelse:\n    grade = "Fail"\n\nprint("Your grade is:", grade)\nprint("Pass!" if marks >= 40 else "Study harder!")' },
        { type:'output', content:'Your grade is: B\nPass!' },
        { type:'tip', content:'Python uses == to compare (not single =). Single = assigns a value; double == checks equality.' },
        { type:'quiz', question:'In Python, what does elif mean?', options:['Error in logic','Else if — another condition to check','A type of loop','A function name'], answer:1, explanation:'elif is short for "else if" — it checks another condition if the previous ones were False.' }
      ]},
      { id:'py4', title:'Loops', topic:'Repeating code', difficulty:'intermediate', sections:[
        { type:'text', content:'Loops let you run the same code multiple times. Python has two main loops: for loop (iterate over a sequence) and while loop (repeat while a condition is True).' },
        { type:'code', content:'# For loop — print 1 to 5\nfor i in range(1, 6):\n    print("Number:", i)\n\nprint("---")\n\n# While loop — countdown\ncountdown = 3\nwhile countdown > 0:\n    print("Countdown:", countdown)\n    countdown -= 1\nprint("Blast off!")' },
        { type:'output', content:'Number: 1\nNumber: 2\nNumber: 3\nNumber: 4\nNumber: 5\n---\nCountdown: 3\nCountdown: 2\nCountdown: 1\nBlast off!' },
        { type:'quiz', question:'What does range(1, 5) produce?', options:['1, 2, 3, 4, 5','1, 2, 3, 4','0, 1, 2, 3, 4, 5','1 to 4 repeatedly'], answer:1, explanation:'range(1, 5) produces 1, 2, 3, 4 — it starts at 1 and goes up to (but NOT including) 5.' }
      ]},
      { id:'py5', title:'Functions', topic:'Reusable code blocks', difficulty:'intermediate', sections:[
        { type:'text', content:'Functions are reusable blocks of code. You define a function once with def and call it whenever needed. Functions can take inputs (parameters) and return outputs.' },
        { type:'code', content:'def greet(name):\n    return "Assalamualaikum, " + name + "!"\n\ndef calculate_grade(marks):\n    if marks >= 80: return "A"\n    elif marks >= 60: return "B"\n    elif marks >= 40: return "C"\n    else: return "Fail"\n\n# Call the functions\nprint(greet("Ahmed"))\nprint(greet("Sara"))\nprint("Grade:", calculate_grade(72))\nprint("Grade:", calculate_grade(35))' },
        { type:'output', content:'Assalamualaikum, Ahmed!\nAssalamualaikum, Sara!\nGrade: B\nGrade: Fail' },
        { type:'quiz', question:'What keyword defines a function in Python?', options:['function','define','def','func'], answer:2, explanation:'In Python, you use "def" followed by the function name and parentheses to define a function.' }
      ]},
    ]
  },
  {
    id: 'html', name: 'HTML & CSS', icon: '🌐', desc: 'Build beautiful web pages', lessons: [
      { id:'h1', title:'HTML Basics', topic:'Structure of a webpage', difficulty:'beginner', sections:[
        { type:'text', content:'HTML (HyperText Markup Language) is the skeleton of every webpage. HTML uses tags (like <h1>, <p>, <div>) to structure content. Every webpage you visit is built with HTML.' },
        { type:'code', lang:'HTML', content:'<!DOCTYPE html>\n<html>\n  <head>\n    <title>My First Page</title>\n  </head>\n  <body>\n    <h1>Assalamualaikum!</h1>\n    <p>Welcome to my first webpage.</p>\n    <p>I am learning HTML today.</p>\n    <a href="#">Click here</a>\n  </body>\n</html>' },
        { type:'tip', content:'All HTML tags come in pairs: opening <tag> and closing </tag>. The content goes in between.' },
        { type:'quiz', question:'What does HTML stand for?', options:['High Text Markup Language','HyperText Markup Language','HyperText Making Language','High Transfer Markup Link'], answer:1, explanation:'HTML = HyperText Markup Language. It structures the content of web pages.' }
      ]},
      { id:'h2', title:'CSS Styling', topic:'Making pages beautiful', difficulty:'beginner', sections:[
        { type:'text', content:'CSS (Cascading Style Sheets) adds design and styling to HTML. CSS controls colors, fonts, spacing, and layout. Without CSS, websites would just be plain black text on a white background.' },
        { type:'code', lang:'CSS', content:'/* CSS to style the page */\nbody {\n  background-color: #f0f4f8;\n  font-family: Arial, sans-serif;\n  margin: 0;\n  padding: 20px;\n}\n\nh1 {\n  color: #1a73e8;\n  font-size: 2rem;\n  text-align: center;\n}\n\n.card {\n  background: white;\n  border-radius: 12px;\n  padding: 20px;\n  box-shadow: 0 2px 10px rgba(0,0,0,0.1);\n}' },
        { type:'quiz', question:'What does CSS stand for?', options:['Computer Style Sheets','Creative Style Sheets','Cascading Style Sheets','Colorful Style Sheets'], answer:2, explanation:'CSS = Cascading Style Sheets. "Cascading" means styles can flow down from parent to child elements.' }
      ]},
      { id:'h3', title:'HTML Forms', topic:'Getting user input', difficulty:'intermediate', sections:[
        { type:'text', content:'HTML forms collect user input — like login forms, search boxes, and registration forms. Forms use <form>, <input>, <button>, and other tags.' },
        { type:'code', lang:'HTML', content:'<form action="/submit" method="post">\n  <label for="name">Your Name:</label>\n  <input type="text" id="name" name="name" placeholder="Enter your name" />\n\n  <label for="email">Email:</label>\n  <input type="email" id="email" name="email" />\n\n  <label for="subject">Subject:</label>\n  <select id="subject" name="subject">\n    <option value="math">Mathematics</option>\n    <option value="physics">Physics</option>\n    <option value="bio">Biology</option>\n  </select>\n\n  <button type="submit">Submit</button>\n</form>' },
        { type:'quiz', question:'Which input type is used for email fields?', options:['type="text"','type="mail"','type="email"','type="contact"'], answer:2, explanation:'type="email" validates that the user enters a properly formatted email address.' }
      ]},
    ]
  },
  {
    id: 'javascript', name: 'JavaScript', icon: '⚡', desc: 'Make websites interactive', lessons: [
      { id:'js1', title:'JavaScript Basics', topic:'Programming for the web', difficulty:'beginner', sections:[
        { type:'text', content:'JavaScript (JS) makes websites interactive. It runs in your browser and can change HTML content, respond to clicks, validate forms, and much more. JavaScript is the most popular programming language in the world.' },
        { type:'code', lang:'JavaScript', content:'// Variables (use const or let)\nconst name = "Hassan";\nlet score = 95;\n\n// Output to browser console\nconsole.log("Hello,", name);\nconsole.log("Score:", score);\n\n// Alert (popup box)\nalert("Welcome to JavaScript!");\n\n// Change HTML content\ndocument.getElementById("title").textContent = "JavaScript is fun!";\n\n// Math operations\nlet total = score + 5;\nconsole.log("Total:", total);' },
        { type:'quiz', question:'What does console.log() do in JavaScript?', options:['Creates a popup','Saves to a file','Prints to browser console','None of the above'], answer:2, explanation:'console.log() prints output to the browser\'s developer console (press F12 to see it).' }
      ]},
      { id:'js2', title:'DOM Manipulation', topic:'Changing webpages dynamically', difficulty:'intermediate', sections:[
        { type:'text', content:'The DOM (Document Object Model) lets JavaScript access and change HTML elements. This is how websites become dynamic — buttons change, content updates without refreshing.' },
        { type:'code', lang:'JavaScript', content:'// Get element by ID\nconst heading = document.getElementById("main-heading");\n\n// Change text\nheading.textContent = "New Title!";\n\n// Change style\nheading.style.color = "blue";\nheading.style.fontSize = "2rem";\n\n// Add event listener (respond to click)\nconst btn = document.getElementById("my-button");\nbtn.addEventListener("click", function() {\n  alert("Button was clicked!");\n  btn.style.backgroundColor = "green";\n});\n\n// Create new element\nconst newPara = document.createElement("p");\nnewPara.textContent = "This was added by JavaScript!";\ndocument.body.appendChild(newPara);' },
        { type:'quiz', question:'What does addEventListener() do?', options:['Adds a new HTML element','Listens for user actions like clicks','Removes an element','Changes CSS style'], answer:1, explanation:'addEventListener() registers a function to be called when a specific event (like click, keypress) happens.' }
      ]},
    ]
  },
  {
    id: 'cpp', name: 'C++', icon: '⚙️', desc: 'Foundation of computer science', lessons: [
      { id:'cpp1', title:'Introduction to C++', topic:'Compiled programming', difficulty:'beginner', sections:[
        { type:'text', content:'C++ is a powerful language used for system programming, game development, and competitive programming. It is the foundation of many modern languages. C++ is fast and gives you fine control over memory.' },
        { type:'code', lang:'C++', content:'#include <iostream>\nusing namespace std;\n\nint main() {\n    // Print text to screen\n    cout << "Assalamualaikum!" << endl;\n    cout << "Welcome to C++" << endl;\n    \n    // Variable and input\n    string name;\n    cout << "Enter your name: ";\n    cin >> name;\n    cout << "Hello, " << name << "!" << endl;\n    \n    return 0; // Program ended successfully\n}' },
        { type:'tip', content:'In C++, every program starts with main() function. #include <iostream> lets you use cout and cin for input/output.' },
        { type:'quiz', question:'What does cout << do in C++?', options:['Reads input from keyboard','Outputs text to screen','Declares a variable','Includes a library'], answer:1, explanation:'cout (pronounced "c-out") is used to output/print text to the screen. << is the stream insertion operator.' }
      ]},
      { id:'cpp2', title:'Variables & Arrays', topic:'Storing collections of data', difficulty:'beginner', sections:[
        { type:'text', content:'Variables store single values. Arrays store multiple values of the same type. Arrays are fundamental for working with lists, matrices, and data collections.' },
        { type:'code', lang:'C++', content:'#include <iostream>\nusing namespace std;\n\nint main() {\n    // Regular variables\n    int marks = 85;\n    float gpa = 3.7;\n    string subject = "Physics";\n    \n    // Array — stores multiple values\n    int scores[5] = {90, 85, 78, 92, 88};\n    \n    // Loop through array\n    int sum = 0;\n    for (int i = 0; i < 5; i++) {\n        cout << "Score " << i+1 << ": " << scores[i] << endl;\n        sum += scores[i];\n    }\n    \n    cout << "Average: " << sum/5 << endl;\n    return 0;\n}' },
        { type:'quiz', question:'What is the index of the FIRST element in a C++ array?', options:['1','0','-1','Depends on the array'], answer:1, explanation:'All C-based languages (C, C++, Java, JavaScript) use 0-based indexing. The first element is at index [0].' }
      ]},
    ]
  },
]
</script>

<style scoped>
.lc-root { max-width: 800px; margin: 0 auto; padding: 1.5rem; }
.lc-hero { text-align: center; padding: 2rem 1rem 1.5rem; }
.lc-hero-icon { font-size: 3rem; margin-bottom: 0.5rem; }
.lc-hero-title { font-size: 1.75rem; font-weight: 800; color: var(--t-text1); }
.lc-hero-sub { color: var(--t-text3); font-size: 0.875rem; }
.lc-lang-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(180px, 1fr)); gap: 1rem; }
.lc-lang-card { border: 1px solid var(--t-border); border-radius: 18px; padding: 1.5rem 1rem; text-align: center; cursor: pointer; transition: all 0.15s; background: var(--t-surface); }
.lc-lang-card:hover { border-color: #4caf50; transform: translateY(-2px); box-shadow: 0 4px 16px rgba(76,175,80,0.15); }
.lc-lang-icon { font-size: 2.5rem; margin-bottom: 0.5rem; }
.lc-lang-name { font-weight: 800; font-size: 1rem; color: var(--t-text1); }
.lc-lang-desc { font-size: 0.78rem; color: var(--t-text3); margin: 0.25rem 0; }
.lc-lang-meta { font-size: 0.72rem; color: #4caf50; font-weight: 600; }
.lc-back { padding: 0.4rem 0.85rem; background: var(--t-hover); border: 1px solid var(--t-border); border-radius: 99px; color: var(--t-text2); font-size: 0.8rem; cursor: pointer; }
.lc-lessons-header { display: flex; align-items: center; gap: 1rem; margin-bottom: 1rem; }
.lc-lessons-title { font-size: 1.25rem; font-weight: 800; color: var(--t-text1); }
.lc-progress-bar { height: 6px; background: var(--t-border); border-radius: 99px; overflow: hidden; margin-bottom: 0.4rem; }
.lc-progress-fill { height: 100%; background: linear-gradient(90deg, #4caf50, #00bcd4); border-radius: 99px; transition: width 0.4s; }
.lc-progress-label { font-size: 0.75rem; color: var(--t-text3); margin-bottom: 1rem; }
.lc-lesson-list { display: flex; flex-direction: column; gap: 0.5rem; }
.lc-lesson-item { display: flex; align-items: center; gap: 1rem; padding: 0.85rem 1rem; border: 1px solid var(--t-border); border-radius: 14px; cursor: pointer; transition: all 0.15s; background: var(--t-surface); }
.lc-lesson-item:hover:not(.locked) { border-color: #4caf50; background: rgba(76,175,80,0.04); }
.lc-lesson-item.locked { opacity: 0.4; cursor: not-allowed; }
.lc-lesson-item.done { border-color: rgba(76,175,80,0.3); background: rgba(76,175,80,0.04); }
.lc-lesson-num { width: 28px; height: 28px; border-radius: 50%; background: var(--t-hover); color: var(--t-text2); font-size: 0.8rem; font-weight: 700; display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
.lc-lesson-item.done .lc-lesson-num { background: #4caf50; color: white; }
.lc-lesson-name { font-weight: 700; color: var(--t-text1); font-size: 0.9rem; }
.lc-lesson-topic { font-size: 0.75rem; color: var(--t-text3); }
.lc-lesson-badge { margin-left: auto; font-size: 0.68rem; font-weight: 700; padding: 0.2rem 0.5rem; border-radius: 99px; }
.lc-lesson-badge.beginner { background: rgba(76,175,80,0.1); color: #4caf50; }
.lc-lesson-badge.intermediate { background: rgba(245,124,0,0.1); color: #f57c00; }
.lc-detail-header { display: flex; align-items: center; gap: 1rem; margin-bottom: 1.5rem; }
.lc-detail-title { font-size: 1.25rem; font-weight: 800; color: var(--t-text1); }
.lc-section { margin-bottom: 1.25rem; }
.lc-section-text { color: var(--t-text1); font-size: 0.95rem; line-height: 1.7; padding: 0.75rem; background: var(--t-hover); border-radius: 12px; }
.lc-section-code { border: 1px solid var(--t-border); border-radius: 12px; overflow: hidden; }
.lc-code-header { padding: 0.5rem 1rem; background: var(--t-hover); font-size: 0.72rem; font-weight: 700; color: var(--t-text3); border-bottom: 1px solid var(--t-border); }
.lc-code-block { padding: 1rem; background: #1a1a2e; color: #e0e0e0; font-family: 'Courier New', monospace; font-size: 0.85rem; line-height: 1.7; overflow-x: auto; margin: 0; white-space: pre; }
.lc-code-ai-btn { width: 100%; padding: 0.5rem; background: linear-gradient(135deg, rgba(109,84,232,0.1), rgba(168,85,247,0.08)); color: #6d54e8; border: none; cursor: pointer; font-size: 0.8rem; font-weight: 600; border-top: 1px solid var(--t-border); }
.lc-code-ai-btn:hover { background: linear-gradient(135deg, rgba(109,84,232,0.2), rgba(168,85,247,0.15)); }
.lc-section-output { border: 1px solid var(--t-border); border-radius: 12px; overflow: hidden; }
.lc-output-header { padding: 0.5rem 1rem; background: var(--t-hover); font-size: 0.72rem; font-weight: 700; color: #4caf50; border-bottom: 1px solid var(--t-border); }
.lc-output-block { padding: 1rem; background: rgba(76,175,80,0.03); color: var(--t-text1); font-family: 'Courier New', monospace; font-size: 0.85rem; line-height: 1.7; margin: 0; white-space: pre; }
.lc-section-tip { padding: 0.75rem 1rem; background: rgba(245,124,0,0.07); border-left: 3px solid #f57c00; border-radius: 0 12px 12px 0; color: var(--t-text1); font-size: 0.88rem; display: flex; gap: 0.5rem; }
.lc-quiz-block { padding: 1rem; border: 1px solid rgba(109,84,232,0.2); border-radius: 12px; background: rgba(109,84,232,0.03); }
.lc-quiz-question { font-weight: 700; color: var(--t-text1); font-size: 0.9rem; margin-bottom: 0.75rem; }
.lc-quiz-options { display: flex; flex-direction: column; gap: 0.4rem; }
.lc-quiz-opt { text-align: left; padding: 0.55rem 1rem; border: 1px solid var(--t-border); border-radius: 9px; background: var(--t-surface); color: var(--t-text2); cursor: pointer; font-size: 0.85rem; transition: all 0.1s; }
.lc-quiz-opt:hover { border-color: #6d54e8; color: #6d54e8; }
.lc-quiz-opt.correct { background: rgba(76,175,80,0.1); border-color: #4caf50; color: #4caf50; font-weight: 700; }
.lc-quiz-opt.wrong { background: rgba(239,68,68,0.1); border-color: #ef4444; color: #ef4444; }
.lc-quiz-opt.right_show { background: rgba(76,175,80,0.08); border-color: rgba(76,175,80,0.3); color: #4caf50; }
.lc-quiz-feedback { margin-top: 0.6rem; padding: 0.5rem 0.75rem; border-radius: 9px; font-size: 0.83rem; background: rgba(239,68,68,0.07); color: #ef4444; }
.lc-quiz-feedback.ok { background: rgba(76,175,80,0.07); color: #4caf50; }
.lc-code-exp { margin: 1rem 0; padding: 1rem; background: linear-gradient(135deg, rgba(109,84,232,0.05), rgba(168,85,247,0.05)); border: 1px solid rgba(109,84,232,0.2); border-radius: 12px; }
.lc-code-exp-header { font-size: 0.78rem; font-weight: 700; color: #6d54e8; margin-bottom: 0.5rem; }
.lc-code-exp-text { font-size: 0.875rem; color: var(--t-text1); line-height: 1.7; white-space: pre-wrap; }
.lc-complete-btn { width: 100%; padding: 0.85rem; background: #4caf50; color: white; border: none; border-radius: 12px; font-weight: 800; font-size: 1rem; cursor: pointer; margin-top: 1rem; }
.lc-complete-btn:disabled { background: var(--t-hover); color: #4caf50; cursor: default; border: 1px solid rgba(76,175,80,0.3); }
</style>
