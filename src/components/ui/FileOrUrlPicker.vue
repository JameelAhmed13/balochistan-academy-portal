<template>
  <div class="fp-root">

    <!-- Mode toggle -->
    <div class="fp-mode-bar">
      <button type="button" :class="['fp-mode-btn', { active: mode === 'upload' }]" @click="mode = 'upload'">
        <UploadCloud class="w-3.5 h-3.5" /> Upload file
      </button>
      <button type="button" :class="['fp-mode-btn', { active: mode === 'url' }]" @click="mode = 'url'">
        <Link2 class="w-3.5 h-3.5" /> Paste URL
      </button>
    </div>

    <!-- ── UPLOAD MODE ─────────────────────────────────────────── -->
    <template v-if="mode === 'upload'">

      <!-- Uploading progress -->
      <div v-if="uploading" class="fp-uploading">
        <div class="fp-progress-track">
          <div class="fp-progress-fill" :style="{ width: progress + '%' }" />
        </div>
        <span class="fp-up-txt">Uploading… {{ progress }}%</span>
      </div>

      <!-- Result — has value -->
      <div v-else-if="modelValue" class="fp-result">
        <div class="fp-result-left">
          <img v-if="isImage"
               :src="modelValue"
               class="fp-result-img"
               @error="e => e.target.style.display = 'none'" />
          <div v-else class="fp-result-file-box">
            <FileText class="w-5 h-5" style="color:var(--t-accent);" />
          </div>
        </div>
        <div class="fp-result-info">
          <span class="fp-result-name" :title="modelValue">{{ shortName }}</span>
          <button type="button" class="fp-result-change" @click="fileInput.click()">
            Replace file
          </button>
        </div>
        <button type="button" class="fp-result-x" @click="clear" title="Remove">
          <X class="w-3.5 h-3.5" />
        </button>
      </div>

      <!-- Drop zone -->
      <div v-else
           class="fp-dropzone"
           :class="{ dragging: isDragging }"
           @click="fileInput.click()"
           @dragover.prevent="isDragging = true"
           @dragleave.prevent="isDragging = false"
           @drop.prevent="onDrop">
        <div class="fp-drop-icon-wrap">
          <UploadCloud class="fp-drop-icon" />
        </div>
        <p class="fp-drop-title">Drop here or <span class="fp-browse">browse</span></p>
        <p class="fp-drop-sub">{{ acceptLabel }}</p>
      </div>

      <p v-if="uploadErr" class="fp-err">{{ uploadErr }}</p>

      <input
        ref="fileInput"
        type="file"
        :accept="accept === '*' ? undefined : accept"
        style="display:none"
        @change="onFileChange"
      />
    </template>

    <!-- ── URL MODE ───────────────────────────────────────────── -->
    <template v-else>
      <div class="fp-url-row">
        <Link2 class="fp-url-icon" />
        <input
          type="url"
          class="fp-url-input"
          :value="modelValue"
          :placeholder="placeholder"
          @input="emit('update:modelValue', $event.target.value)"
        />
        <button v-if="modelValue" type="button" class="fp-result-x" @click="clear" title="Clear">
          <X class="w-3.5 h-3.5" />
        </button>
      </div>
      <!-- Image preview when URL looks like an image -->
      <div v-if="isImage && modelValue" class="fp-url-preview-wrap">
        <img
          :src="modelValue"
          class="fp-url-preview"
          @error="e => e.target.parentElement.style.display = 'none'"
        />
      </div>
    </template>

  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { UploadCloud, Link2, FileText, X } from '@lucide/vue'
import api from '@/services/api'

const props = defineProps({
  modelValue: { type: String, default: '' },
  accept:     { type: String, default: '*' },    // e.g. 'image/*', '*', 'application/pdf'
  folder:     { type: String, default: 'misc' }, // upload sub-folder on the server
  placeholder:{ type: String, default: 'https://…' },
})
const emit = defineEmits(['update:modelValue'])

// Start in URL mode if a value is already set (edit), otherwise upload mode
const mode       = ref(props.modelValue ? 'url' : 'upload')
const isDragging = ref(false)
const uploading  = ref(false)
const progress   = ref(0)
const uploadErr  = ref('')
const fileInput  = ref(null)

const isImage = computed(() => {
  const v = props.modelValue
  if (!v) return false
  if (props.accept === 'image/*') return true
  return /\.(jpg|jpeg|png|gif|webp|svg|bmp)(\?.*)?$/i.test(v)
})

const shortName = computed(() => {
  const v = props.modelValue
  if (!v) return ''
  const name = v.split('/').pop()?.split('?')[0] ?? v
  return name.length > 44 ? name.slice(0, 41) + '…' : name
})

const acceptLabel = computed(() => {
  if (props.accept === 'image/*') return 'PNG, JPG, GIF, WebP, SVG · max 50 MB'
  if (props.accept === 'application/pdf') return 'PDF files · max 50 MB'
  return 'PDF, images, Word, PPT, MP4 · max 50 MB'
})

function onDrop(e) {
  isDragging.value = false
  const file = e.dataTransfer?.files?.[0]
  if (file) uploadFile(file)
}

function onFileChange(e) {
  const file = e.target?.files?.[0]
  if (file) uploadFile(file)
  if (fileInput.value) fileInput.value.value = '' // allow re-selecting same file
}

async function uploadFile(file) {
  uploading.value = true
  progress.value  = 0
  uploadErr.value = ''
  try {
    const fd = new FormData()
    fd.append('file', file)
    const res = await api.post(`/admin/upload?folder=${props.folder}`, fd, {
      headers: { 'Content-Type': 'multipart/form-data' },
      onUploadProgress(e) {
        if (e.total) progress.value = Math.round((e.loaded / e.total) * 100)
      },
    })
    emit('update:modelValue', res.data.url)
    mode.value = 'upload' // stay in upload mode — show result box
  } catch (e) {
    uploadErr.value = e.message || 'Upload failed'
  } finally {
    uploading.value = false
  }
}

function clear() {
  emit('update:modelValue', '')
  uploadErr.value = ''
  mode.value = 'upload'
}
</script>

<style scoped>
.fp-root {
  display: flex; flex-direction: column; gap: 8px;
}

/* Mode toggle */
.fp-mode-bar {
  display: flex; gap: 4px;
  padding: 4px; border-radius: 10px;
  background: var(--t-bg); border: 1px solid var(--t-border);
  width: fit-content;
}
.fp-mode-btn {
  display: inline-flex; align-items: center; gap: 5px;
  padding: 5px 12px; border-radius: 7px; border: none; cursor: pointer;
  font-size: 0.78rem; font-weight: 600; font-family: inherit;
  color: var(--t-text3); background: transparent;
  transition: all 0.15s;
}
.fp-mode-btn:hover { color: var(--t-text1); background: var(--t-bg2); }
.fp-mode-btn.active {
  color: var(--t-accent); background: var(--t-acc-alpha-sm);
}

/* Drop zone */
.fp-dropzone {
  display: flex; flex-direction: column; align-items: center; justify-content: center;
  gap: 6px; padding: 24px 16px;
  border: 2px dashed var(--t-border); border-radius: 12px;
  cursor: pointer; transition: all 0.18s;
  background: var(--t-bg);
}
.fp-dropzone:hover, .fp-dropzone.dragging {
  border-color: var(--t-accent);
  background: var(--t-acc-alpha-sm);
}
.fp-drop-icon-wrap {
  width: 44px; height: 44px; border-radius: 12px;
  background: var(--t-acc-alpha-sm);
  display: flex; align-items: center; justify-content: center;
}
.fp-drop-icon { width: 22px; height: 22px; color: var(--t-accent); }
.fp-drop-title {
  font-size: 0.85rem; font-weight: 600; color: var(--t-text2); margin: 0;
}
.fp-browse { color: var(--t-accent); text-decoration: underline; cursor: pointer; }
.fp-drop-sub { font-size: 0.75rem; color: var(--t-text3); margin: 0; }

/* Upload progress */
.fp-uploading {
  display: flex; flex-direction: column; gap: 6px; padding: 14px;
  border-radius: 12px; background: var(--t-bg); border: 1px solid var(--t-border);
}
.fp-progress-track {
  height: 6px; border-radius: 999px; background: var(--t-border); overflow: hidden;
}
.fp-progress-fill {
  height: 100%; border-radius: 999px; background: var(--t-accent);
  transition: width 0.2s ease;
}
.fp-up-txt { font-size: 0.8rem; color: var(--t-text3); }

/* Result box */
.fp-result {
  display: flex; align-items: center; gap: 10px; padding: 10px 12px;
  border-radius: 12px; border: 1px solid var(--t-border); background: var(--t-bg);
}
.fp-result-left { flex-shrink: 0; }
.fp-result-img {
  width: 52px; height: 40px; object-fit: cover; border-radius: 6px;
  border: 1px solid var(--t-border);
}
.fp-result-file-box {
  width: 40px; height: 40px; border-radius: 8px;
  background: var(--t-acc-alpha-sm);
  display: flex; align-items: center; justify-content: center;
}
.fp-result-info {
  flex: 1; min-width: 0; display: flex; flex-direction: column; gap: 3px;
}
.fp-result-name {
  font-size: 0.8rem; font-weight: 600; color: var(--t-text1);
  white-space: nowrap; overflow: hidden; text-overflow: ellipsis;
}
.fp-result-change {
  font-size: 0.72rem; color: var(--t-accent); background: none; border: none;
  cursor: pointer; padding: 0; text-align: left; font-family: inherit; font-weight: 600;
}
.fp-result-change:hover { text-decoration: underline; }
.fp-result-x {
  flex-shrink: 0; display: inline-flex; align-items: center; justify-content: center;
  width: 28px; height: 28px; border-radius: 8px; border: none; cursor: pointer;
  background: transparent; color: var(--t-text3); transition: all 0.15s;
}
.fp-result-x:hover { background: #f8717120; color: #f87171; }

/* URL mode */
.fp-url-row {
  display: flex; align-items: center; gap: 6px;
  padding: 0 12px; border-radius: 10px;
  border: 1px solid var(--t-border); background: var(--t-bg);
  transition: border-color 0.15s;
}
.fp-url-row:focus-within { border-color: var(--t-accent); box-shadow: 0 0 0 3px var(--t-acc-alpha-sm); }
.fp-url-icon { width: 14px; height: 14px; flex-shrink: 0; color: var(--t-text3); }
.fp-url-input {
  flex: 1; padding: 10px 0; border: none; background: transparent;
  color: var(--t-text1); font-size: 0.875rem; font: inherit; outline: none;
  min-width: 0;
}
.fp-url-input::placeholder { color: var(--t-text3); }
.fp-url-preview-wrap {
  border-radius: 10px; overflow: hidden; border: 1px solid var(--t-border);
  max-height: 120px; display: flex; align-items: center; justify-content: center;
  background: var(--t-bg);
}
.fp-url-preview { max-width: 100%; max-height: 120px; object-fit: contain; display: block; }

/* Error */
.fp-err { font-size: 0.8rem; color: #f87171; margin: 0; }
</style>
