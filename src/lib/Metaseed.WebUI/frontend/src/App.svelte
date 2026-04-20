<script>
  import HotkeySearch from './components/HotkeySearch.svelte'
  import LogViewer from './components/LogViewer.svelte'
  import CloseButton from './components/CloseButton.svelte'
  import { onMount } from 'svelte'
  import { initMessageListeners, sendClose } from './lib/messaging.js'

  const MAX_LOGS = 2000

  let activeView = null // 'search' | 'logs' | null
  let hotkeys = []
  let logs = []

  function handleClose() {
    sendClose()
    activeView = null
  }

  function addLog(log) {
    logs = logs.length >= MAX_LOGS
      ? [...logs.slice(logs.length - MAX_LOGS + 1), log]
      : [...logs, log]
  }

  onMount(() => {
    const cleanup = initMessageListeners({
      onShowSearch(data) {
        activeView = 'search'
        if (Array.isArray(data.hotkeys)) {
          hotkeys = data.hotkeys
        }
        setTimeout(() => document.dispatchEvent(new CustomEvent('focus-search')), 50)
      },

      onShowLogs(data) {
        activeView = 'logs'
        if (Array.isArray(data.logs)) {
          logs = data.logs
        }
      },

      onAddLog(data) {
        if (data.log) {
          addLog(data.log)
        }
      }
    })

    return cleanup
  })
</script>

{#if activeView === 'search'}
  <!-- <div class="fixed inset-0 bg-gray-100 p-2 relative group overflow-hidden flex flex-col"> -->
    <!-- <CloseButton onClick={handleClose} /> -->
    <HotkeySearch {hotkeys} onClose={handleClose} />
  <!-- </div> -->
{:else if activeView === 'logs'}
  <div class="h-screen bg-gray-900">
    <LogViewer bind:logs onClose={handleClose} />
  </div>
{/if}
