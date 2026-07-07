<template>
  <div class="animate-fade-in space-y-5">

    <!-- Page Header -->
    <div class="ag-page-hd">
      <div class="ag-page-hd-left">
        <button class="btn-ghost" @click="goBack"><ArrowLeft class="w-4 h-4" /></button>
        <div>
          <h2 class="section-title">Topics & Resources</h2>
          <p v-if="unit" class="ut-sub">{{ unit.name }}</p>
        </div>
      </div>
      <button class="btn-primary text-sm" @click="openAddTopic">
        <Plus class="w-4 h-4" /> Add Topic
      </button>
    </div>

    <!-- Banner -->
    <div class="ag-banner">
      <div class="ag-banner-icon">📚</div>
      <div class="ag-banner-text">
        <div class="ag-banner-title">{{ unit?.name ?? 'Unit' }}</div>
        <div class="ag-banner-sub">Topics and their learning resources</div>
      </div>
      <div class="ag-banner-actions">
        <span class="ag-banner-stat">📝 {{ topics.length }} Topic{{ topics.length !== 1 ? 's' : '' }}</span>
        <span class="ag-banner-stat">📎 {{ totalResources }} Resource{{ totalResources !== 1 ? 's' : '' }}</span>
      </div>
    </div>

    <!-- Topics Accordion -->
    <div class="ag-card" style="padding:0;overflow:hidden;">

      <!-- Loading -->
      <div v-if="loading" class="ag-empty" style="padding:40px;">
        <div class="ag-empty-icon">⏳</div><p>Loading topics…</p>
      </div>

      <!-- Empty -->
      <div v-else-if="topics.length === 0" class="ag-empty" style="padding:48px 24px;">
        <div class="ag-empty-icon">📝</div>
        <p style="margin-bottom:12px;">No topics yet for this unit.</p>
        <button class="btn-primary text-sm" @click="openAddTopic">
          <Plus class="w-4 h-4" /> Add first topic
        </button>
      </div>

      <!-- Accordion list -->
      <div v-else>
        <div
          v-for="(topic, idx) in topics"
          :key="topic.id"
          class="tp-section"
          :class="{ 'tp-section--expanded': expandedTopics[topic.id] }"
        >
          <!-- Topic header row -->
          <div class="tp-header" @click="toggleTopic(topic.id)">
            <div class="tp-header-left">
              <ChevronDown
                class="tp-chevron"
                :style="expandedTopics[topic.id] ? '' : 'transform:rotate(-90deg)'"
              />
              <span class="tp-index">{{ idx + 1 }}</span>
              <div class="tp-title-wrap">
                <span class="tp-name">{{ topic.name }}</span>
                <span class="tp-meta">Sort: {{ topic.sortOrder }}</span>
              </div>
            </div>
            <div class="tp-header-right">
              <span class="tp-res-badge">
                📎 {{ topic.resourceCount ?? 0 }} resource{{ topic.resourceCount !== 1 ? 's' : '' }}
              </span>
              <div class="tp-actions" @click.stop>
                <button class="btn-ghost" style="padding:0.3rem;" title="Edit topic" @click="openEditTopic(topic)">
                  <Pencil class="w-3.5 h-3.5" />
                </button>
                <button class="btn-ghost" style="padding:0.3rem;color:#f87171;" title="Delete topic" @click="removeTopic(topic)">
                  <Trash2 class="w-3.5 h-3.5" />
                </button>
              </div>
            </div>
          </div>

          <!-- Resources panel (expanded) -->
          <div v-if="expandedTopics[topic.id]" class="tp-body">

            <div class="tp-body-head">
              <span class="tp-body-title">Resources for "{{ topic.name }}"</span>
              <button class="btn-primary text-sm" style="padding:6px 14px;" @click="openAddResource(topic)">
                <Plus class="w-3.5 h-3.5" /> Add Resource
              </button>
            </div>

            <!-- Loading resources -->
            <div v-if="topicLoading[topic.id]" class="tp-res-loading">
              <span>⏳</span> Loading resources…
            </div>

            <!-- No resources -->
            <div v-else-if="!(topicResources[topic.id]?.length)" class="tp-res-empty">
              <span style="font-size:1.6rem;">📎</span>
              <p>No resources yet for this topic.</p>
              <button class="btn-secondary text-sm" @click="openAddResource(topic)">
                <Plus class="w-3.5 h-3.5" /> Add first resource
              </button>
            </div>

            <!-- Resources table -->
            <div v-else class="overflow-x-auto">
              <table class="ag-table">
                <thead>
                  <tr>
                    <th style="width:40px;">#</th>
                    <th style="min-width:160px;">Title</th>
                    <th style="width:90px;">Kind</th>
                    <th style="min-width:150px;">URL</th>
                    <th style="min-width:90px;">Thumbnail</th>
                    <th style="min-width:120px;">Tags</th>
                    <th style="width:76px;">Duration</th>
                    <th style="width:64px;">Year</th>
                    <th style="width:60px;">Order</th>
                    <th style="width:68px;">Status</th>
                    <th style="width:76px;">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="(r, ri) in topicResources[topic.id]" :key="r.id">
                    <td class="ag-table-muted">{{ ri + 1 }}</td>
                    <td>
                      <div style="font-weight:600;color:var(--t-text1);font-size:0.875rem;">{{ r.title }}</div>
                      <div v-if="r.description" class="ag-table-muted" style="font-size:0.75rem;margin-top:2px;">{{ r.description }}</div>
                    </td>
                    <td>
                      <span :class="kindClass(r.kind)" style="font-size:0.72rem;">{{ r.kind || '—' }}</span>
                    </td>
                    <td style="min-width:150px;">
                      <button v-if="r.url" class="ut-view-btn" @click="openViewer(r)" :title="r.url">
                        <Eye class="w-3 h-3" />
                        <span class="ut-view-url">{{ shortUrl(r.url) }}</span>
                      </button>
                      <span v-else class="ag-table-muted">—</span>
                    </td>
                    <td>
                      <template v-if="r.thumbnailUrl">
                        <img :src="r.thumbnailUrl" class="ut-thumb" @error="e => e.target.style.display='none'" alt="" />
                      </template>
                      <span v-else class="ag-table-muted">—</span>
                    </td>
                    <td style="min-width:120px;">
                      <template v-if="r.tags">
                        <span v-for="tag in r.tags.split(',')" :key="tag" class="ut-tag">{{ tag.trim() }}</span>
                      </template>
                      <span v-else class="ag-table-muted">—</span>
                    </td>
                    <td class="ag-table-muted" style="white-space:nowrap;">
                      {{ r.durationSec ? formatDuration(r.durationSec) : '—' }}
                    </td>
                    <td class="ag-table-muted">{{ r.sourceYear || '—' }}</td>
                    <td class="ag-table-muted">{{ r.sortOrder }}</td>
                    <td>
                      <span :class="r.isPublished ? 'badge-blue' : 'badge-amber'" style="font-size:0.72rem;">
                        {{ r.isPublished ? 'Live' : 'Draft' }}
                      </span>
                    </td>
                    <td>
                      <div style="display:flex;gap:0.25rem;">
                        <button class="btn-ghost" style="padding:0.3rem;" title="Edit" @click="openEditResource(topic, r)">
                          <Pencil class="w-3.5 h-3.5" />
                        </button>
                        <button class="btn-ghost" style="padding:0.3rem;color:#f87171;" title="Delete" @click="removeResource(topic, r)">
                          <Trash2 class="w-3.5 h-3.5" />
                        </button>
                      </div>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- ── RESOURCE VIEWER ── -->
    <div v-if="viewerResource" class="vw-overlay" @click.self="viewerResource = null">
      <div class="vw-dialog">

        <!-- Viewer header -->
        <div class="vw-header">
          <div class="vw-header-left">
            <span :class="kindClass(viewerResource.kind)" style="font-size:0.72rem;">{{ viewerResource.kind }}</span>
            <span class="vw-title">{{ viewerResource.title }}</span>
          </div>
          <div class="vw-header-right">
            <a :href="viewerResource.url" target="_blank" rel="noopener" class="vw-ext-btn" title="Open in new tab">
              <ExternalLink class="w-3.5 h-3.5" /> Open in new tab
            </a>
            <button class="vw-close" @click="viewerResource = null" title="Close">
              <X class="w-4 h-4" />
            </button>
          </div>
        </div>

        <!-- Viewer body -->
        <div class="vw-body" :class="`vw-body--${viewerType}`">

          <!-- ── YouTube ── -->
          <iframe
            v-if="viewerType === 'youtube'"
            :src="viewerEmbedUrl"
            class="vw-iframe"
            allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
            allowfullscreen
          />

          <!-- ── Vimeo ── -->
          <iframe
            v-else-if="viewerType === 'vimeo'"
            :src="viewerEmbedUrl"
            class="vw-iframe"
            allow="autoplay; fullscreen; picture-in-picture"
            allowfullscreen
          />

          <!-- ── Google Slides / Docs ── -->
          <iframe
            v-else-if="viewerType === 'google-slides' || viewerType === 'google-docs'"
            :src="viewerEmbedUrl"
            class="vw-iframe"
            allowfullscreen
          />

          <!-- ── Office: PPT / DOCX / XLSX  (MS → Google → Download) ── -->
          <template v-else-if="viewerType === 'office'">

            <!-- Local uploads can't reach online viewers -->
            <div v-if="isLocalUrl(viewerResource.url)" class="vw-no-preview">
              <FileText class="vw-no-preview-icon" style="color:var(--t-text3);" />
              <p class="vw-no-preview-msg">Office files need a public URL to preview.</p>
              <p class="vw-no-preview-url">Local uploads can't be sent to online viewers.<br>Download and open in PowerPoint / Word / Excel.</p>
              <a :href="viewerResource.url" download class="btn-primary" style="margin-top:8px;">⬇ Download file</a>
            </div>

            <div v-else class="vw-office-wrap">

              <!-- Step indicator bar -->
              <div class="vw-office-bar">
                <span class="vw-office-bar-label">Viewer:</span>

                <button
                  v-for="(step, i) in officeSteps" :key="i"
                  :class="['vw-office-step', { active: officeViewerStep === i, done: officeViewerStep > i }]"
                  @click="officeViewerStep = i"
                >
                  <span class="vw-step-dot">{{ i + 1 }}</span>
                  {{ step }}
                </button>
              </div>

              <!-- Iframe viewer (steps 0 & 1) -->
              <iframe
                v-if="officeViewerStep < 2"
                :src="officeIframeUrl"
                :key="officeViewerStep"
                class="vw-iframe vw-iframe--pdf"
                frameborder="0"
              />

              <!-- Download card (step 2) -->
              <div v-else class="vw-no-preview">
                <FileText class="vw-no-preview-icon" style="color:var(--t-text3);" />
                <p class="vw-no-preview-msg">All online viewers failed.</p>
                <p class="vw-no-preview-url">{{ viewerResource.url }}</p>
                <a :href="viewerResource.url" download class="btn-primary" style="margin-top:8px;">⬇ Download file</a>
              </div>

              <!-- "Not loading?" hint -->
              <div v-if="officeViewerStep < 2" class="vw-office-hint">
                Not loading?
                <button class="vw-office-next" @click="officeViewerStep++">
                  {{ officeViewerStep === 0 ? 'Try Google Docs Viewer' : 'Download file' }} →
                </button>
              </div>

            </div>
          </template>

          <!-- ── PDF (browser native) ── -->
          <iframe
            v-else-if="viewerType === 'pdf'"
            :src="viewerResource.url"
            class="vw-iframe vw-iframe--pdf"
          />

          <!-- ── Native Video ── -->
          <video
            v-else-if="viewerType === 'video'"
            controls
            class="vw-video"
            :key="viewerResource.url"
          >
            <source :src="viewerResource.url" />
            Your browser does not support the video tag.
          </video>

          <!-- ── Audio ── -->
          <div v-else-if="viewerType === 'audio'" class="vw-audio-wrap">
            <div class="vw-audio-card">
              <Music class="vw-audio-icon" />
              <p class="vw-audio-title">{{ viewerResource.title }}</p>
              <audio controls class="vw-audio" :key="viewerResource.url">
                <source :src="viewerResource.url" />
              </audio>
            </div>
          </div>

          <!-- ── Image ── -->
          <div v-else-if="viewerType === 'image'" class="vw-image-wrap">
            <img
              :src="viewerResource.url"
              :alt="viewerResource.title"
              class="vw-image"
              @error="e => e.target.src = ''"
            />
          </div>

          <!-- ── Generic iframe (slides, exercises, etc.) ── -->
          <iframe
            v-else-if="viewerType === 'embed'"
            :src="viewerResource.url"
            class="vw-iframe vw-iframe--pdf"
            sandbox="allow-scripts allow-same-origin allow-forms allow-popups"
          />

          <!-- ── No preview (external link, unknown) ── -->
          <div v-else class="vw-no-preview">
            <FileText class="vw-no-preview-icon" style="color:var(--t-text3);" />
            <p class="vw-no-preview-msg">This resource cannot be previewed inline.</p>
            <p class="vw-no-preview-url">{{ viewerResource.url }}</p>
            <a :href="viewerResource.url" target="_blank" rel="noopener" class="btn-primary" style="margin-top:8px;">
              <ExternalLink class="w-4 h-4" /> Open in new tab
            </a>
          </div>

        </div>

        <!-- Viewer footer (description / tags) -->
        <div v-if="viewerResource.description || viewerResource.tags" class="vw-footer">
          <p v-if="viewerResource.description" class="vw-desc">{{ viewerResource.description }}</p>
          <div v-if="viewerResource.tags" class="vw-tags">
            <span v-for="tag in viewerResource.tags.split(',')" :key="tag" class="ut-tag">{{ tag.trim() }}</span>
          </div>
        </div>

      </div>
    </div>

    <!-- ── TOPIC MODAL ── -->
    <div v-if="topicForm" class="ut-modal" @click.self="topicForm = null">
      <div class="ut-dialog">
        <h2>{{ editingTopic ? 'Edit topic' : 'Add topic' }}</h2>

        <label>
          Topic Name <span class="ut-req">*</span>
          <input v-model="topicForm.name" type="text" placeholder="e.g. Introduction to Cells" />
        </label>

        <label>
          Sort Order
          <input v-model.number="topicForm.sortOrder" type="number" min="0" placeholder="0" />
        </label>

        <p v-if="topicErr" class="ut-err">{{ topicErr }}</p>
        <div class="ut-foot">
          <button class="btn-secondary" @click="topicForm = null">Cancel</button>
          <button class="btn-primary" :disabled="topicSaving" @click="saveTopic">
            {{ topicSaving ? 'Saving…' : (editingTopic ? 'Save changes' : 'Add topic') }}
          </button>
        </div>
      </div>
    </div>

    <!-- ── RESOURCE MODAL ── -->
    <div v-if="resourceForm" class="ut-modal" @click.self="resourceForm = null">
      <div class="ut-dialog" style="width:min(560px,100%);">
        <h2>{{ editingResource ? 'Edit resource' : 'Add resource' }}</h2>
        <p class="ut-dialog-sub">Topic: <strong>{{ activeTopic?.name }}</strong></p>

        <label>
          Title <span class="ut-req">*</span>
          <input v-model="resourceForm.title" type="text" placeholder="e.g. Cell Biology PDF" />
        </label>

        <div class="ut-row">
          <label style="flex:1;">
            Kind <span class="ut-req">*</span>
            <select v-model="resourceForm.kind">
              <option value="">— select kind —</option>
              <option v-for="k in RESOURCE_KINDS" :key="k.value" :value="k.value">{{ k.label }}</option>
            </select>
          </label>
          <label style="flex:1;">
            Source Year
            <input v-model="resourceForm.sourceYear" type="text" placeholder="e.g. 2024" />
          </label>
        </div>

        <div class="ut-field">
          URL / File <span class="ut-req">*</span>
          <FileOrUrlPicker v-model="resourceForm.url" folder="resources" placeholder="https://…" />
        </div>

        <div class="ut-field">
          Thumbnail Image
          <FileOrUrlPicker v-model="resourceForm.thumbnailUrl" accept="image/*" folder="images" placeholder="https://…/thumb.jpg" />
        </div>

        <label>
          Description
          <textarea v-model="resourceForm.description" rows="2" placeholder="Brief description…" />
        </label>

        <div class="ut-row">
          <label style="flex:1;">
            Duration (seconds)
            <input v-model.number="resourceForm.durationSec" type="number" min="0" placeholder="e.g. 300" />
          </label>
          <label style="flex:1;">
            Sort Order
            <input v-model.number="resourceForm.sortOrder" type="number" min="0" placeholder="0" />
          </label>
        </div>

        <label>
          Tags (comma-separated)
          <input v-model="resourceForm.tags" type="text" placeholder="e.g. biology, cells, chapter1" />
        </label>

        <label class="ut-toggle-label">
          <input type="checkbox" v-model="resourceForm.isPublished" class="ut-checkbox" />
          Published (visible to students)
        </label>

        <p v-if="resourceErr" class="ut-err">{{ resourceErr }}</p>
        <div class="ut-foot">
          <button class="btn-secondary" @click="resourceForm = null">Cancel</button>
          <button class="btn-primary" :disabled="resourceSaving" @click="saveResource">
            {{ resourceSaving ? 'Saving…' : (editingResource ? 'Save changes' : 'Add resource') }}
          </button>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ArrowLeft, Plus, Pencil, Trash2, ChevronDown, Eye, ExternalLink, X, FileText, Music, PlayCircle, FileImage } from '@lucide/vue'
import { useCatalogStore } from '@/stores/catalog'
import { useConfirm } from '@/composables/useConfirm'
import FileOrUrlPicker from '@/components/ui/FileOrUrlPicker.vue'

const route   = useRoute()
const router  = useRouter()
const catalog = useCatalogStore()
const confirm = useConfirm()

const unitId    = computed(() => Number(route.params.unitId))
const bookId    = computed(() => Number(route.params.bookId))
const subjectId = computed(() => Number(route.params.subjectId))

const unit    = ref(null)
const topics  = ref([])
const loading = ref(false)

// Per-topic resource state
const expandedTopics  = ref({}) // topicId → boolean
const topicResources  = ref({}) // topicId → ContentItem[]
const topicLoading    = ref({}) // topicId → boolean

// Topic modal
const topicForm    = ref(null)
const editingTopic = ref(false)
const topicSaving  = ref(false)
const topicErr     = ref('')

// Resource modal
const resourceForm    = ref(null)
const editingResource = ref(false)
const activeTopic     = ref(null) // which topic the resource modal is for
const resourceSaving  = ref(false)
const resourceErr     = ref('')

const totalResources = computed(() =>
  topics.value.reduce((s, t) => s + (t.resourceCount ?? 0), 0)
)

const RESOURCE_KINDS = [
  { value: 'pdf',        label: '📄 PDF' },
  { value: 'video',      label: '🎬 Video' },
  { value: 'link',       label: '🔗 Link' },
  { value: 'image',      label: '🖼️ Image' },
  { value: 'audio',      label: '🎵 Audio' },
  { value: 'slides',     label: '📊 Slides' },
  { value: 'exercise',   label: '✏️ Exercise' },
  { value: 'past-paper', label: '📋 Past Paper' },
]

const KIND_CLASSES = {
  pdf: 'badge-rose', video: 'badge-blue', link: 'badge-blue',
  image: 'badge-amber', audio: 'badge-amber', slides: 'badge-violet',
  exercise: 'badge-emerald', 'past-paper': 'badge-amber',
}
function kindClass(kind) { return KIND_CLASSES[kind] ?? 'badge-blue' }

// ── Resource viewer ──────────────────────────────────────────────────────────
const viewerResource  = ref(null)
const officeViewerStep = ref(0) // 0 = Microsoft, 1 = Google, 2 = Download

watch(() => viewerResource.value?.id, () => { officeViewerStep.value = 0 })

function shortUrl(url) {
  if (!url) return ''
  try { const u = new URL(url); return (u.hostname + u.pathname).slice(0, 38) + (url.length > 38 ? '…' : '') }
  catch { return url.slice(0, 38) + (url.length > 38 ? '…' : '') }
}

function absoluteUrl(url) {
  try { return new URL(url, window.location.origin).href }
  catch { return url }
}

function isLocalUrl(url) {
  try {
    const { hostname } = new URL(url, window.location.origin)
    return hostname === 'localhost' || hostname === '127.0.0.1' || hostname.startsWith('127.')
  } catch {
    return true
  }
}

function detectViewerType(r) {
  const url  = r.url || ''
  const kind = r.kind || ''
  // Video platforms
  if (/youtube\.com|youtu\.be/.test(url))          return 'youtube'
  if (/vimeo\.com/.test(url))                      return 'vimeo'
  // Google
  if (/docs\.google\.com\/presentation/.test(url)) return 'google-slides'
  if (/docs\.google\.com/.test(url))               return 'google-docs'
  // Office documents — use MS Office Online Viewer (needs public URL)
  if (/\.(pptx?|docx?|xlsx?)(\?|#|$)/i.test(url)) return 'office'
  // By extension
  if (/\.(pdf)(\?|#|$)/i.test(url))                    return 'pdf'
  if (/\.(mp4|webm|ogv|mov)(\?|#|$)/i.test(url))       return 'video'
  if (/\.(mp3|wav|ogg|m4a|aac)(\?|#|$)/i.test(url))    return 'audio'
  if (/\.(jpe?g|png|gif|webp|svg|bmp)(\?|#|$)/i.test(url)) return 'image'
  // By kind
  if (kind === 'pdf')        return 'pdf'
  if (kind === 'video')      return 'video'
  if (kind === 'audio')      return 'audio'
  if (kind === 'image')      return 'image'
  if (kind === 'slides')     return 'office'
  if (kind === 'exercise')   return 'embed'
  if (kind === 'past-paper') return 'pdf'
  return 'none'
}

const viewerType = computed(() => {
  if (!viewerResource.value?.url) return 'none'
  return detectViewerType(viewerResource.value)
})

const viewerEmbedUrl = computed(() => {
  const r = viewerResource.value
  if (!r?.url) return ''
  const type = viewerType.value
  if (type === 'youtube') {
    const m = r.url.match(/(?:youtube\.com\/(?:watch\?v=|shorts\/)|youtu\.be\/)([A-Za-z0-9_-]{11})/)
    return m ? `https://www.youtube.com/embed/${m[1]}?rel=0` : r.url
  }
  if (type === 'vimeo') {
    const m = r.url.match(/vimeo\.com\/(\d+)/)
    return m ? `https://player.vimeo.com/video/${m[1]}` : r.url
  }
  if (type === 'google-slides') return r.url.replace(/\/(edit|view)[^/]*$/, '/embed')
  if (type === 'google-docs')   return r.url.replace(/\/(edit|view)[^/]*$/, '/preview')
  return r.url
})

// Microsoft → Google fallback URLs for Office docs
const officeIframeUrl = computed(() => {
  const url = viewerResource.value?.url
  if (!url) return ''
  const abs = absoluteUrl(url)
  if (officeViewerStep.value === 0)
    return `https://view.officeapps.live.com/op/embed.aspx?src=${encodeURIComponent(abs)}`
  if (officeViewerStep.value === 1)
    return `https://docs.google.com/viewer?url=${encodeURIComponent(abs)}&embedded=true`
  return ''
})

const officeSteps = ['Microsoft Office', 'Google Docs', 'Download']

function openViewer(r) { viewerResource.value = r }

function formatDuration(sec) {
  if (!sec) return '—'
  const h = Math.floor(sec / 3600), m = Math.floor((sec % 3600) / 60), s = sec % 60
  if (h > 0) return `${h}h ${m}m`
  if (m > 0) return `${m}m ${s}s`
  return `${s}s`
}

function goBack() {
  router.push(`/app/admin/content/${subjectId.value}/books/${bookId.value}/units`)
}

onMounted(async () => {
  loading.value = true
  try {
    const units = await catalog.fetchBookUnits(bookId.value)
    unit.value  = units.find(u => u.id === unitId.value) ?? null
    topics.value = await catalog.fetchUnitTopics(unitId.value)
  } finally {
    loading.value = false
  }
})

// ── Toggle expand ────────────────────────────────────────────────────────────
async function toggleTopic(topicId) {
  const nowOpen = !expandedTopics.value[topicId]
  expandedTopics.value = { ...expandedTopics.value, [topicId]: nowOpen }
  if (nowOpen && topicResources.value[topicId] === undefined) {
    await loadTopicResources(topicId)
  }
}

async function loadTopicResources(topicId) {
  topicLoading.value = { ...topicLoading.value, [topicId]: true }
  try {
    topicResources.value = {
      ...topicResources.value,
      [topicId]: await catalog.fetchTopicResources(topicId),
    }
  } finally {
    topicLoading.value = { ...topicLoading.value, [topicId]: false }
  }
}

// ── Topic CRUD ───────────────────────────────────────────────────────────────
function openAddTopic() {
  editingTopic.value = false; topicErr.value = ''
  topicForm.value = { name: '', sortOrder: topics.value.length }
}

function openEditTopic(topic) {
  editingTopic.value = true; topicErr.value = ''
  topicForm.value = { id: topic.id, name: topic.name, sortOrder: topic.sortOrder ?? 0 }
}

async function saveTopic() {
  if (!topicForm.value.name.trim()) { topicErr.value = 'Topic name is required'; return }
  topicSaving.value = true; topicErr.value = ''
  try {
    const payload = { name: topicForm.value.name.trim(), sortOrder: topicForm.value.sortOrder ?? 0 }
    if (editingTopic.value) {
      await catalog.updateUnitTopic(unitId.value, topicForm.value.id, payload)
    } else {
      await catalog.createUnitTopic(unitId.value, payload)
    }
    topics.value = await catalog.fetchUnitTopics(unitId.value)
    topicForm.value = null
  } catch (e) {
    topicErr.value = e.response?.data?.error || e.message
  } finally {
    topicSaving.value = false
  }
}

async function removeTopic(topic) {
  const ok = await confirm({
    title: `Delete "${topic.name}"`,
    message: 'This removes the topic and all its resources permanently.',
    confirmLabel: 'Delete Topic',
  })
  if (!ok) return
  try {
    await catalog.deleteUnitTopic(unitId.value, topic.id)
    topics.value = topics.value.filter(t => t.id !== topic.id)
    // Clean up cached resources
    const { [topic.id]: _r, ...restRes } = topicResources.value
    topicResources.value = restRes
    const { [topic.id]: _e, ...restExp } = expandedTopics.value
    expandedTopics.value = restExp
  } catch (e) { alert(e.response?.data?.error || e.message) }
}

// ── Resource CRUD ────────────────────────────────────────────────────────────
function openAddResource(topic) {
  activeTopic.value     = topic
  editingResource.value = false
  resourceErr.value     = ''
  resourceForm.value = {
    title: '', kind: '', url: '', thumbnailUrl: '', description: '',
    durationSec: null, sortOrder: topicResources.value[topic.id]?.length ?? 0,
    tags: '', sourceYear: '', isPublished: false,
  }
}

function openEditResource(topic, r) {
  activeTopic.value     = topic
  editingResource.value = true
  resourceErr.value     = ''
  resourceForm.value = {
    id:           r.id,
    title:        r.title,
    kind:         r.kind ?? '',
    url:          r.url ?? '',
    thumbnailUrl: r.thumbnailUrl ?? '',
    description:  r.description ?? '',
    durationSec:  r.durationSec ?? null,
    sortOrder:    r.sortOrder ?? 0,
    tags:         r.tags ?? '',
    sourceYear:   r.sourceYear ?? '',
    isPublished:  r.isPublished ?? false,
  }
}

async function saveResource() {
  if (!resourceForm.value.title.trim()) { resourceErr.value = 'Title is required'; return }
  if (!resourceForm.value.kind)         { resourceErr.value = 'Kind is required'; return }
  resourceSaving.value = true; resourceErr.value = ''
  const topic = activeTopic.value
  try {
    const payload = {
      title:        resourceForm.value.title.trim(),
      kind:         resourceForm.value.kind,
      url:          resourceForm.value.url || null,
      thumbnailUrl: resourceForm.value.thumbnailUrl || null,
      description:  resourceForm.value.description || null,
      durationSec:  resourceForm.value.durationSec || null,
      sortOrder:    resourceForm.value.sortOrder ?? 0,
      tags:         resourceForm.value.tags || null,
      sourceYear:   resourceForm.value.sourceYear || null,
      isPublished:  resourceForm.value.isPublished,
    }
    if (editingResource.value) {
      await catalog.updateTopicResource(topic.id, resourceForm.value.id, payload)
    } else {
      await catalog.createTopicResource(topic.id, payload)
    }
    // Refresh this topic's resource list
    topicResources.value = {
      ...topicResources.value,
      [topic.id]: await catalog.fetchTopicResources(topic.id),
    }
    // Update resourceCount badge on the topic row
    const t = topics.value.find(t => t.id === topic.id)
    if (t) t.resourceCount = topicResources.value[topic.id].length

    resourceForm.value = null
  } catch (e) {
    resourceErr.value = e.response?.data?.error || e.message
  } finally {
    resourceSaving.value = false
  }
}

async function removeResource(topic, r) {
  const ok = await confirm({
    title: `Delete "${r.title}"`,
    message: 'This will permanently remove the resource.',
    confirmLabel: 'Delete Resource',
  })
  if (!ok) return
  try {
    await catalog.deleteTopicResource(topic.id, r.id)
    topicResources.value = {
      ...topicResources.value,
      [topic.id]: topicResources.value[topic.id].filter(x => x.id !== r.id),
    }
    const t = topics.value.find(t => t.id === topic.id)
    if (t) t.resourceCount = Math.max(0, (t.resourceCount ?? 1) - 1)
  } catch (e) { alert(e.response?.data?.error || e.message) }
}
</script>

<style scoped>
.ut-sub { font-size: 0.78rem; color: var(--t-text3); margin-top: 0.1rem; }

/* ── Topic accordion ─────────────────────────────────────────── */
.tp-section {
  border-bottom: 1px solid var(--t-border);
}
.tp-section:last-child { border-bottom: none; }

.tp-header {
  display: flex; align-items: center; justify-content: space-between;
  padding: 14px 20px; cursor: pointer; user-select: none;
  transition: background 0.15s;
}
.tp-header:hover { background: var(--t-bg); }
.tp-section--expanded .tp-header { background: var(--t-acc-alpha-sm); }

.tp-header-left { display: flex; align-items: center; gap: 10px; flex: 1; min-width: 0; }
.tp-chevron {
  width: 16px; height: 16px; color: var(--t-text3); flex-shrink: 0;
  transition: transform 0.2s;
}
.tp-index {
  min-width: 28px; height: 28px; border-radius: 8px;
  background: var(--t-acc-alpha-sm); color: var(--t-accent);
  display: inline-flex; align-items: center; justify-content: center;
  font-size: 0.78rem; font-weight: 700; flex-shrink: 0;
}
.tp-title-wrap { min-width: 0; }
.tp-name { font-weight: 700; color: var(--t-text1); font-size: 0.9rem; display: block; }
.tp-meta { font-size: 0.72rem; color: var(--t-text3); }

.tp-header-right { display: flex; align-items: center; gap: 10px; flex-shrink: 0; }
.tp-res-badge {
  font-size: 0.75rem; font-weight: 600; color: var(--t-text3);
  background: var(--t-bg); border: 1px solid var(--t-border);
  padding: 3px 10px; border-radius: 999px;
}
.tp-section--expanded .tp-res-badge { color: var(--t-accent); border-color: var(--t-accent); background: var(--t-acc-alpha-sm); }
.tp-actions { display: flex; gap: 4px; }

/* ── Resources panel ─────────────────────────────────────────── */
.tp-body {
  border-top: 1px solid var(--t-border);
  background: var(--t-bg);
}
.tp-body-head {
  display: flex; align-items: center; justify-content: space-between;
  padding: 12px 20px;
  border-bottom: 1px solid var(--t-border);
}
.tp-body-title { font-size: 0.82rem; font-weight: 600; color: var(--t-text2); }

.tp-res-loading {
  display: flex; align-items: center; gap: 8px; padding: 20px;
  font-size: 0.85rem; color: var(--t-text3);
}
.tp-res-empty {
  display: flex; flex-direction: column; align-items: center;
  gap: 10px; padding: 32px 20px;
  font-size: 0.85rem; color: var(--t-text3);
}
.tp-res-empty p { margin: 0; }

/* ── Table helpers ───────────────────────────────────────────── */
.ut-link {
  color: var(--t-accent); text-decoration: none;
  overflow: hidden; text-overflow: ellipsis; white-space: nowrap; display: block; max-width: 140px;
}
.ut-link:hover { text-decoration: underline; }
.ut-thumb {
  display: block; width: 44px; height: 30px; object-fit: cover;
  border-radius: 4px; border: 1px solid var(--t-border);
}
.ut-tag {
  display: inline-block; padding: 1px 6px; margin: 1px 2px 1px 0;
  border-radius: 999px; font-size: 0.68rem; font-weight: 600;
  background: var(--t-acc-alpha-sm); color: var(--t-accent); white-space: nowrap;
}

/* ── URL view button ─────────────────────────────────────────── */
.ut-view-btn {
  display: inline-flex; align-items: center; gap: 5px;
  background: none; border: none; cursor: pointer; padding: 3px 0;
  color: var(--t-accent); font-size: 0.78rem; font-family: inherit;
  max-width: 160px; text-align: left;
}
.ut-view-btn:hover { text-decoration: underline; }
.ut-view-url { overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }

/* ── Resource Viewer ─────────────────────────────────────────── */
.vw-overlay {
  position: fixed; inset: 0; z-index: 200;
  display: flex; align-items: center; justify-content: center;
  padding: 20px;
  background: rgba(0,0,0,.72);
  backdrop-filter: blur(6px);
}
.vw-dialog {
  display: flex; flex-direction: column;
  width: min(960px, 100%); max-height: 92vh;
  border-radius: 18px; overflow: hidden;
  background: var(--t-bg2); border: 1px solid var(--t-border);
  box-shadow: 0 32px 80px rgba(0,0,0,.5);
}
.vw-header {
  display: flex; align-items: center; justify-content: space-between; gap: 12px;
  padding: 14px 20px; border-bottom: 1px solid var(--t-border);
  flex-shrink: 0;
}
.vw-header-left { display: flex; align-items: center; gap: 10px; min-width: 0; }
.vw-title {
  font-weight: 700; color: var(--t-text1); font-size: 0.95rem;
  white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 480px;
}
.vw-header-right { display: flex; align-items: center; gap: 8px; flex-shrink: 0; }
.vw-ext-btn {
  display: inline-flex; align-items: center; gap: 5px;
  padding: 6px 12px; border-radius: 8px;
  font-size: 0.78rem; font-weight: 600; font-family: inherit;
  color: var(--t-accent); background: var(--t-acc-alpha-sm);
  border: 1px solid var(--t-accent); text-decoration: none;
  transition: background 0.15s;
}
.vw-ext-btn:hover { background: var(--t-acc-alpha); }
.vw-close {
  display: inline-flex; align-items: center; justify-content: center;
  width: 32px; height: 32px; border-radius: 8px; border: none;
  background: var(--t-bg); color: var(--t-text2); cursor: pointer;
  transition: all 0.15s;
}
.vw-close:hover { background: #f8717120; color: #f87171; }

.vw-body {
  flex: 1; min-height: 0; display: flex; align-items: stretch;
  background: #000;
}
/* 16:9 for video/slides */
.vw-body--youtube,
.vw-body--vimeo,
.vw-body--google-slides,
.vw-body--video {
  aspect-ratio: 16 / 9;
  background: #000;
}
/* Tall for PDF/embed/office */
.vw-body--pdf,
.vw-body--embed,
.vw-body--office,
.vw-body--google-docs {
  min-height: 60vh;
  background: var(--t-bg);
}
/* Fit content */
.vw-body--image,
.vw-body--audio,
.vw-body--none { background: var(--t-bg); }

.vw-iframe {
  width: 100%; height: 100%; border: none; display: block; flex: 1;
}
.vw-iframe--pdf { min-height: 65vh; }

.vw-video {
  width: 100%; max-height: 65vh; display: block; background: #000;
}

/* Audio */
.vw-audio-wrap {
  flex: 1; display: flex; align-items: center; justify-content: center;
  padding: 40px 24px;
}
.vw-audio-card {
  display: flex; flex-direction: column; align-items: center; gap: 16px;
  width: 100%; max-width: 460px;
  padding: 32px; border-radius: 16px;
  background: var(--t-bg2); border: 1px solid var(--t-border);
}
.vw-audio-icon { width: 48px; height: 48px; color: var(--t-accent); }
.vw-audio-title { font-weight: 600; color: var(--t-text1); margin: 0; text-align: center; }
.vw-audio { width: 100%; }

/* Image */
.vw-image-wrap {
  flex: 1; display: flex; align-items: center; justify-content: center;
  padding: 20px; background: var(--t-bg);
}
.vw-image { max-width: 100%; max-height: 68vh; object-fit: contain; border-radius: 8px; }

/* No preview */
.vw-no-preview {
  flex: 1; display: flex; flex-direction: column; align-items: center;
  justify-content: center; gap: 10px; padding: 48px 24px;
}
.vw-no-preview-icon { width: 48px; height: 48px; }
.vw-no-preview-msg { font-size: 0.95rem; font-weight: 600; color: var(--t-text1); margin: 0; }
.vw-no-preview-url {
  font-size: 0.78rem; color: var(--t-text3); margin: 0;
  word-break: break-all; max-width: 480px; text-align: center;
}

/* ── Office viewer fallback chain ────────────────────────────── */
.vw-office-wrap { display: flex; flex-direction: column; flex: 1; min-height: 0; }

.vw-office-bar {
  display: flex; align-items: center; gap: 6px; flex-shrink: 0;
  padding: 8px 16px; background: var(--t-bg2); border-bottom: 1px solid var(--t-border);
}
.vw-office-bar-label { font-size: 0.75rem; font-weight: 600; color: var(--t-text3); margin-right: 4px; }

.vw-office-step {
  display: inline-flex; align-items: center; gap: 6px;
  padding: 5px 12px; border-radius: 8px; border: 1px solid var(--t-border);
  background: var(--t-bg); color: var(--t-text2);
  font-size: 0.78rem; font-weight: 600; font-family: inherit; cursor: pointer;
  text-decoration: none; transition: all 0.15s;
}
.vw-office-step:hover { border-color: var(--t-accent); color: var(--t-accent); }
.vw-office-step.active {
  background: var(--t-accent); color: #fff; border-color: var(--t-accent);
}
.vw-office-step.done { opacity: 0.45; }

.vw-step-dot {
  width: 16px; height: 16px; border-radius: 50%;
  background: rgba(255,255,255,0.25); color: inherit;
  font-size: 0.65rem; font-weight: 800;
  display: inline-flex; align-items: center; justify-content: center;
}
.vw-office-step:not(.active) .vw-step-dot {
  background: var(--t-border); color: var(--t-text3);
}

.vw-office-hint {
  flex-shrink: 0; padding: 7px 16px;
  background: var(--t-bg); border-top: 1px solid var(--t-border);
  font-size: 0.78rem; color: var(--t-text3);
  display: flex; align-items: center; gap: 8px;
}
.vw-office-next {
  background: none; border: none; cursor: pointer; padding: 0;
  font-size: 0.78rem; font-weight: 600; font-family: inherit;
  color: var(--t-accent); text-decoration: underline;
}
.vw-office-next:hover { opacity: 0.8; }

/* Footer */
.vw-footer {
  padding: 12px 20px; border-top: 1px solid var(--t-border);
  display: flex; flex-direction: column; gap: 6px; flex-shrink: 0;
  background: var(--t-bg2);
}
.vw-desc { font-size: 0.82rem; color: var(--t-text2); margin: 0; }
.vw-tags { display: flex; flex-wrap: wrap; gap: 4px; }

/* ── Modals ──────────────────────────────────────────────────── */
.ut-modal {
  position: fixed; inset: 0; z-index: 100;
  display: grid; place-items: center;
  background: rgba(0,0,0,.5); padding: 20px;
  backdrop-filter: blur(4px);
}
.ut-dialog {
  width: min(500px, 100%); max-height: 92vh; overflow-y: auto;
  padding: 28px; border-radius: 18px;
  background: var(--t-bg2); border: 1px solid var(--t-border);
  display: flex; flex-direction: column; gap: 14px;
  box-shadow: var(--t-shadow-md);
}
.ut-dialog h2 { font-family: 'Syne', sans-serif; margin: 0; font-size: 1.25rem; color: var(--t-text1); }
.ut-dialog-sub { font-size: 0.8rem; color: var(--t-text3); margin: -8px 0 0; }
.ut-dialog label {
  display: flex; flex-direction: column; gap: 5px;
  font-size: .82rem; font-weight: 600; color: var(--t-text2);
}
.ut-dialog input, .ut-dialog select, .ut-dialog textarea {
  padding: 10px 12px; border-radius: 10px;
  border: 1px solid var(--t-border);
  background: var(--t-bg); color: var(--t-text1); font-size: .9rem; font: inherit;
  resize: vertical;
}
.ut-dialog input:focus, .ut-dialog select:focus, .ut-dialog textarea:focus {
  outline: none; border-color: var(--t-accent);
  box-shadow: 0 0 0 3px var(--t-acc-alpha-sm);
}
.ut-field {
  display: flex; flex-direction: column; gap: 5px;
  font-size: .82rem; font-weight: 600; color: var(--t-text2);
}
.ut-row { display: flex; gap: 12px; }
.ut-toggle-label { flex-direction: row !important; align-items: center; gap: 8px !important; font-size: .85rem !important; }
.ut-checkbox { width: 16px; height: 16px; cursor: pointer; accent-color: var(--t-accent); }
.ut-req { color: #f87171; }
.ut-err { color: #f87171; font-size: .85rem; margin: 0; }
.ut-foot { display: flex; justify-content: flex-end; gap: 10px; margin-top: 4px; }
</style>
