import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useConfirmModalStore = defineStore('confirmModal', () => {
  const visible      = ref(false)
  const title        = ref('')
  const message      = ref('')
  const confirmLabel = ref('Confirm')
  const cancelLabel  = ref('Cancel')
  const type         = ref('danger')   // 'danger' | 'warning' | 'info'
  let _resolve = null

  function show(opts = {}) {
    title.value        = opts.title        ?? 'Are you sure?'
    message.value      = opts.message      ?? ''
    confirmLabel.value = opts.confirmLabel ?? 'Confirm'
    cancelLabel.value  = opts.cancelLabel  ?? 'Cancel'
    type.value         = opts.type         ?? 'danger'
    visible.value      = true
    return new Promise((resolve) => { _resolve = resolve })
  }

  function accept() { visible.value = false; _resolve?.(true)  }
  function reject() { visible.value = false; _resolve?.(false) }

  return { visible, title, message, confirmLabel, cancelLabel, type, show, accept, reject }
})
