<script>
  import SearchBar from './components/SearchBar.svelte'
  import { onMount } from 'svelte'

  let show = false
  let hotkeys = []
  let filteredHotkeys = []
  function handleClose() {
    sendMessage({ type: 'close' })
  }

  function handleSearch(query) {
    console.log('Search query:', query)

    // Filter hotkeys based on query
    if (query.trim() === '') {
      filteredHotkeys = hotkeys
    } else {
      filteredHotkeys = hotkeys.filter(item =>
        item.hotkey.toLowerCase().includes(query.toLowerCase()) ||
        item.description.toLowerCase().includes(query.toLowerCase())
      )
    }
  }

  function handleSelection(selectedItem) {
    console.log('Selected item:', selectedItem)
    sendMessage({
      type: 'hotkeySelected',
      hotkey: selectedItem.hotkey,
      description: selectedItem.description
    })
  }

  function sendMessage(data) {
    if (window.chrome && window.chrome.webview) {
      window.chrome.webview.postMessage(data)
      console.log('Message sent to backend:', data)
    } else {
      console.warn('WebView2 API not available')
    }
  }

  onMount(() => {
    // Listen for messages from the WPF host
    if (window.chrome && window.chrome.webview) {
      window.chrome.webview.addEventListener('message', (event) => {
        const data = event.data
        console.log('Message from backend:', data)

        if (data.type === 'showSearch') {
          console.log('Show search triggered by backend')
          show = true
          // Data should be an array of hotkey items
          if (Array.isArray(data.hotkeys)) {
            hotkeys = data.hotkeys
            filteredHotkeys = hotkeys
          }
        }
      })
      console.log('WebView2 message listener initialized')
    } else {
      console.warn('WebView2 API not available - running in non-WebView2 environment')
    }

    // Also listen via window.addEventListener for broader compatibility
    const handleMessage = (event) => {
      if (event.data && event.data.type === 'showSearch') {
        console.log('Show search triggered via window message')
        show = true
        if (Array.isArray(event.data.hotkeys)) {
          hotkeys = event.data.hotkeys
          filteredHotkeys = hotkeys
        }
        document.dispatchEvent(new CustomEvent('focus-search'));
      }
    }
    window.addEventListener('message', handleMessage)

    return () => {
      window.removeEventListener('message', handleMessage)
    }
  })
</script>

<div class="min-h-screen bg-gray-100 p-2 relative group">
    <button
      class="absolute top-1 right-1 w-6 h-6 flex items-center justify-center rounded hover:bg-gray-300 text-gray-500 hover:text-gray-700 transition-all opacity-0 group-hover:opacity-100"
      on:click={handleClose}
      title="Close"
    >
      <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
      </svg>
    </button>
    <div class="mt-4 p-0 bg-white rounded">
      <SearchBar
        {hotkeys}
        {filteredHotkeys}
        onSearch={handleSearch}
        onSelection={handleSelection}
        onClose={() => show = false}
      />
    </div>
</div>
