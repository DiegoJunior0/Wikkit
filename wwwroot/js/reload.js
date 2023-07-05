// reload.js
let isReloading = false;
let startY = 0;
let distanceY = 0;
const showthreshold = 10;
const refreshthreshold = 150; // Adjust the threshold as needed (in pixels)

let reloadContentInstance = null;

window.addEventListener('touchstart', (event) => {
    const touch = event.touches[0];
    startY = touch.clientY;
});

window.addEventListener('touchmove', (event) => {
    const touch = event.touches[0];

    distanceY = touch.clientY - startY;

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

});

window.addEventListener('touchend', (event) => {

    hideRefreshIcon();

    const main = document.getElementsByTagName("main")[0];

    if (!main) return;

    if (reloadContentInstance == null) return;

    if (distanceY > refreshthreshold && main.scrollTop == 0 && !isReloading) {

        isReloading = true;
        distanceY = 0;

        reloadContentInstance.invokeMethodAsync('ReloadContent')
            .then(() => {
                isReloading = false;
            });
    }
    
});

export function setReloadContentInstance(instance) {
    reloadContentInstance = instance;
}

function showRefreshIcon() {
    const iconElement = document.querySelector('.pull-to-refresh-icon');
    const moveMax = refreshthreshold;
    const move = (distanceY > moveMax ? moveMax : distanceY) / 2;

    iconElement.style.display = 'block';
    iconElement.style.top = 'calc(1rem + ' + move + 'px)';
    iconElement.style.transform = 'rotate(' + move * 4 + 'deg)';
    iconElement.style.opacity= distanceY / refreshthreshold;
}

function hideRefreshIcon() {
    const iconElement = document.querySelector('.pull-to-refresh-icon');
    iconElement.style.display = 'none';
}
