<template>
  <div class="animate-fade-in max-w-2xl space-y-6">
    <div class="card p-6 space-y-5">
      <div class="flex items-center gap-3">
        <div class="w-10 h-10 rounded-xl bg-rose-100 flex items-center justify-center text-xl">💬</div>
        <div>
          <h2 class="section-title">Complaints & Suggestions</h2>
          <p class="text-sm text-slate-500">Share your feedback to help us improve</p>
        </div>
      </div>

      <form @submit.prevent="submit" class="space-y-4">
        <div class="grid grid-cols-2 gap-3">
          <div>
            <label class="label">Type</label>
            <select v-model="form.type" class="input">
              <option>Complaint</option>
              <option>Suggestion</option>
              <option>Bug Report</option>
              <option>Feature Request</option>
              <option>General Feedback</option>
            </select>
          </div>
          <div>
            <label class="label">Subject / Module</label>
            <select v-model="form.module" class="input">
              <option>General</option>
              <option>AI Tutor</option>
              <option>Preparation</option>
              <option>Tests</option>
              <option>Coins & Rewards</option>
              <option>Admin Panel</option>
            </select>
          </div>
        </div>
        <div>
          <label class="label">Title</label>
          <input v-model="form.title" type="text" class="input" placeholder="Brief description" required />
        </div>
        <div>
          <label class="label">Details</label>
          <textarea v-model="form.message" class="input min-h-[120px] resize-y" placeholder="Describe your complaint or suggestion in detail…" required />
        </div>
        <button type="submit" class="btn-primary"><Send class="w-4 h-4" /> Submit Feedback</button>
      </form>
    </div>

    <!-- Previous submissions -->
    <div class="card p-5" v-if="submissions.length">
      <h3 class="font-semibold text-slate-700 mb-3">Previous Submissions</h3>
      <div class="space-y-3">
        <div v-for="s in submissions" :key="s.id" class="p-3 rounded-xl bg-slate-50 border border-slate-200">
          <div class="flex items-center gap-2 mb-1">
            <span class="badge-indigo text-xs">{{ s.type }}</span>
            <span class="text-xs text-slate-500">{{ fmt(s.date) }}</span>
            <span class="ml-auto badge-amber text-xs">{{ s.status }}</span>
          </div>
          <div class="font-medium text-sm text-slate-700">{{ s.title }}</div>
          <div class="text-xs text-slate-500 mt-0.5 line-clamp-2">{{ s.message }}</div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { Send } from '@lucide/vue'
import { useToast } from 'primevue/usetoast'

const toast = useToast()
const form = ref({ type: 'Complaint', module: 'General', title: '', message: '' })
const submissions = ref(JSON.parse(localStorage.getItem('bap_complaints') || '[]'))

function submit() {
  const s = { ...form.value, id: Date.now(), date: new Date().toISOString(), status: 'Pending' }
  submissions.value.unshift(s)
  localStorage.setItem('bap_complaints', JSON.stringify(submissions.value))
  form.value = { type: 'Complaint', module: 'General', title: '', message: '' }
  toast.add({ severity: 'success', summary: 'Submitted', detail: 'Thank you for your feedback!', life: 3000 })
}

function fmt(iso) { return new Date(iso).toLocaleDateString('en-PK', { dateStyle: 'medium' }) }
</script>

