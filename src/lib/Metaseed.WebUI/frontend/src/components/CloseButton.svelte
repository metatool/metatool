<script>
  export let onClick = () => {};
</script>

<button
  class="absolute top-1 right-1 w-6 h-6 flex items-center justify-center rounded hover:bg-gray-300 text-gray-500 hover:text-gray-700 transition-all opacity-0 group-hover:opacity-100"
  on:click={onClick}
  title="Close"
>
  <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
  </svg>
</button>
