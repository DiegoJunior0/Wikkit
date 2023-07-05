
export function setTheme(themeName) {
    document.documentElement.setAttribute('data-theme', themeName);
}

export function setFontSize(fontSizeName) {
    document.documentElement.setAttribute('data-font-size', fontSizeName);
}

export function clearFocus(element) {
    element.blur();
}
