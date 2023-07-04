// reload.js
let isReloading = false;
let startY = 0;
const showthreshold = 10;
const refreshthreshold = 100; // Adjust the threshold as needed (in pixels)

let reloadContentInstance = null;

window.addEventListener('touchstart', (event) => {
    const touch = event.touches[0];
    startY = touch.clientY;
});

window.addEventListener('touchmove', (event) => {
    const touch = event.touches[0];
    const distanceY = touch.clientY - startY;

    const main = document.getElementsByTagName("main")[0];

    if (!main) return;

    if (main.scrollTop > 0) {
        startY = touch.clientY;
        return;
    }

    if (distanceY > showthreshold && !isReloading) {
        // Show the pull-to-refresh icon
        showRefreshIcon();
    } else {
        // Hide the icon if pulled back
        hideRefreshIcon();
    }

    if (reloadContentInstance == null) return;

    if (distanceY > refreshthreshold && main.scrollTop == 0 && !isReloading) {
        // Call the C# method to reload the page content
        isReloading = true;
        hideRefreshIcon(); // Hide the icon before reloading

        reloadContentInstance.invokeMethodAsync('ReloadContent')
            .then(() => {
                isReloading = false;
            });
    }

});

window.addEventListener('touchend', (event) => {
    hideRefreshIcon();
});

export function setReloadContentInstance(instance) {
    reloadContentInstance = instance;
}

function showRefreshIcon() {
    const iconElement = document.querySelector('.pull-to-refresh-icon');
    iconElement.style.display = 'block';
}

function hideRefreshIcon() {
    const iconElement = document.querySelector('.pull-to-refresh-icon');
    iconElement.style.display = 'none';
}
