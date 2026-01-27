var IsChild = 0;
var PromptID;
var IsArchived;
var IsFavorite;
var IsToday;
var curSelection;
var popup;

$(document).ready(function () {

    var shareContent = $("#hdnContent").val();
    if (shareContent) {
    } else {
        // Redirect if the browser is not Chrome or Edge
        if (!isAllowedBrowser()) {
            window.location.replace("/BrowserError");
        }
    }

});

function isAllowedBrowser() {
    var userAgent = navigator.userAgent;

    var isChrome = userAgent.includes("Chrome") && !userAgent.includes("Chromium") && !userAgent.includes("OPR");
    var isEdge = userAgent.includes("Edg");

    return isChrome || isEdge;
}

//Close Popup 
function closeContentPopup() {
    var contentPopup = document.getElementById('fileContent');
    if (contentPopup) {
        contentPopup.style.display = "none"; // Hide the popup

        var video = document.querySelector("#fileContent video");

        if (video) {
            video.pause(); // Pause the video
            video.currentTime = 0; // Reset the video to the beginning
        }
        else {
            document.querySelector("#fileContent iframe").src = "";
        }
        // Get the current full URL
        let currentUrl = window.location.href;

        // Remove the last path segment (everything after the final slash)
        let baseUrl = currentUrl.replace(/\/[^/]+$/, "/");

        // Redirect to the cleaned base URL
        window.location.href = baseUrl;
    }
}

//For share the file of azure blob
function ShareFile(shareURL, fileName, platformSocial) 
{
    var platform = platformSocial.toLowerCase()
    switch (platform) {
        case "facebook":
            platformLink = `https://www.facebook.com/sharer/sharer.php?u=${encodeURIComponent(shareURL)}`;
            break;
        case "x":
            platformLink = `https://twitter.com/intent/tweet?url=${encodeURIComponent(shareURL)}`;
            break;
        case "linkedin":
            platformLink = `https://www.linkedin.com/shareArticle?mini=true&url=${encodeURIComponent(shareURL)}`;
            break;
        case "whatsapp":
            platformLink = `https://wa.me/?text=${encodeURIComponent(shareURL)}`;
            break;
        case "mail":
            platformLink = `mailto:?subject=Check this out&body=${encodeURIComponent(shareURL)}`;
            break;

        default:
            console.error("Unsupported platform:", platform);
            return;
    }

    // Open the appropriate share link in a new tab
    window.open(platformLink, '_blank');
}

const video = document.getElementById("ContentVideo");
const playBtn = document.getElementById("PlayButton");

function PlayContentVideo(imgElement) {
    if (video.paused) {
        video.play();
        imgElement.style.display = "none"; // Hide play button
    } else {
        video.pause();
        imgElement.style.display = "block"; // Show play button if paused manually
    }
}

// Listen for play and pause events from native controls too
video.addEventListener("play", () => {
    playBtn.style.display = "none";
});

video.addEventListener("pause", () => {
    playBtn.style.display = "block";
});

video.addEventListener("ended", () => {
    playBtn.style.display = "block";
});

