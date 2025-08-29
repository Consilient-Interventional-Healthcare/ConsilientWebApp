document.addEventListener("DOMContentLoaded", function () {
    const existingRadio = document.getElementById("useExisting");
    const newRadio = document.getElementById("createNew");
    const newFields = document.getElementById("newPatientFields");

    function toggleFields() {
        newFields.style.display = newRadio.checked ? "block" : "none";
    }

    existingRadio.addEventListener("change", toggleFields);
    newRadio.addEventListener("change", toggleFields);
    toggleFields();
});

//(function () {
//    const existingRadio = document.getElementById('useExisting');
//    const newRadio = document.getElementById('createNew');
//    const existingSelect = document.getElementById('SelectedPatientId');
//    const newFields = document.getElementById('newPatientFields');

//    function setDisabled(container, disabled) {
//        container.querySelectorAll('input, select, textarea').forEach(el => {
//            el.disabled = disabled;
//        });
//    }

//    function toggleMode() {
//        const usingExisting = existingRadio.checked;

//        // Existing mode
//        existingSelect.disabled = !usingExisting;

//        // New mode
//        newFields.style.display = usingExisting ? 'none' : 'block';
//        setDisabled(newFields, usingExisting);

//        // If switching to new, clear the existing select to avoid posting a value
//        if (!usingExisting) existingSelect.value = '';

//        // Re-parse unobtrusive validation so disabled fields are ignored
//        if (window.jQuery && $.validator && $.validator.unobtrusive) {
//            const form = $('form');
//            form.removeData('validator');
//            form.removeData('unobtrusiveValidation');
//            $.validator.unobtrusive.parse(form);
//        }
//    }

//    existingRadio.addEventListener('change', toggleMode);
//    newRadio.addEventListener('change', toggleMode);
//    toggleMode(); // initialize
//})();