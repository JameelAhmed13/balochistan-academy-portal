import { useConfirmModalStore } from '@/stores/confirmModal'

export function useConfirm() {
  const store = useConfirmModalStore()
  return (opts) => store.show(opts)
}
