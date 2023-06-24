
document.addEventListener("DOMContentLoaded", function () {

    var last_scroll_top = 0;
    window.addEventListener('scroll', function () {
        
        el_autohide = document.querySelector('.autohide');        

        if (el_autohide) {
            el_height = el_autohide.offsetHeight;
            let scroll_top = window.scrollY;
            if (scroll_top > last_scroll_top) {
                el_autohide.classList.remove('scrolled-up');
                el_autohide.classList.add('scrolled-down');
            }
            else {
                el_autohide.classList.remove('scrolled-down');
                el_autohide.classList.add('scrolled-up');                
            }
            last_scroll_top = scroll_top;
        }
            
    });  

});
