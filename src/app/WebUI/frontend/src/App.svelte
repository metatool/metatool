<script>
  import SearchBar from './components/SearchBar.svelte'
  import { onMount } from 'svelte'

  let show = false
  let results = []

  function toggleShow() {
    show = !show
    console.log('Toggled show to', show)
  }

  function handleSearch(e) {
    const query = e.detail
    console.log('Search query:', query)
    sendMessage({
      type: 'searchPerformed',
      query: query
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
          show = true
          console.log('Show search triggered by backend')
        }
      })
      console.log('WebView2 message listener initialized')
    } else {
      console.warn('WebView2 API not available - running in non-WebView2 environment')
    }

    // Also listen via window.addEventListener for broader compatibility
    const handleMessage = (event) => {
      if (event.data && event.data.type === 'showSearch') {
        show = true
        console.log('Show search triggered via window message')
      }
    }
    window.addEventListener('message', handleMessage)

    return () => {
      window.removeEventListener('message', handleMessage)
    }
  })
</script>

<div class="min-h-screen bg-gray-100 p-4">
  <button on:click={toggleShow} class="bg-blue-500 text-white px-4 py-2 rounded">
    Toggle Search ({show ? 'ON' : 'OFF'})
  </button>

  {#if show}
    <div class="mt-4 p-4 bg-white rounded">
      <SearchBar on:search={handleSearch} on:close={() => show = false} />
    </div>
  {/if}
</div>
