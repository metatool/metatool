<script>
  import SearchBar from './components/SearchBar.svelte'
  import { onMount } from 'svelte'

  let show = false
  let hotkeys = []
  let filteredHotkeys = []

  function handleSearch(e) {
    const query = e.detail
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

  function handleSelection(e) {
    const selectedItem = e.detail
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
      }
    }
    window.addEventListener('message', handleMessage)

    return () => {
      window.removeEventListener('message', handleMessage)
    }
  })
</script>

<div class="min-h-screen bg-gray-100 p-4">
  {#if show}
    <div class="mt-4 p-4 bg-white rounded">
      <SearchBar
        {hotkeys}
        {filteredHotkeys}
        on:search={handleSearch}
        on:selection={handleSelection}
        on:close={() => show = false}
      />
    </div>
  {/if}
</div>
