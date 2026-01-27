
//Close Popup
function closeContentPopup() {
    var contentPopup = document.getElementById('fileContent');
    if (contentPopup) {
        contentPopup.style.display = "none"; // Hide the popup

        var iframe = document.querySelector(".import-file-wrapper iframe");
        if (iframe) {
            iframe.src = ""; // remove the src
        }
    }
}
function goBack() {
    if (document.referrer) {
        window.location.href = document.referrer;
    } else {
        window.location.href = '/'; // fallback if no referrer is available
    }
}
