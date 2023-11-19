document.querySelector('.search-input').addEventListener('input', function (e) {
    const searchTerm = e.target.value.toLowerCase();
    const selectElement = document.querySelector('.custom-select');
    for (const option of selectElement.options) {
        const tokens = option.getAttribute('data-tokens');
        if (option.textContent.toLowerCase().includes(searchTerm) || (tokens && tokens.includes(searchTerm))) {
            option.style.display = '';
        } else {
            option.style.display = 'none';
        }
    }
});

document.querySelector('.search-input').addEventListener('click', function () {
    const selectElement = document.querySelector('.custom-select');
    selectElement.size = selectElement.length;
});

document.querySelector('.custom-select')
    .addEventListener('change', function () {
        const selectedOption = this.options[this.selectedIndex];
        document.querySelector('.search-input').value = selectedOption.text;
        this.size = 0;
    });
