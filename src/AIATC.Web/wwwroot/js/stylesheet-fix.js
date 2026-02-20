// Fix for stylesheet loading issues in deployed environments
(function() {
    // Check if stylesheets are loading properly
    function checkStylesheets() {
        var links = document.querySelectorAll('link[rel="stylesheet"]');
        var baseUrl = document.querySelector('base').href;
        
        links.forEach(function(link) {
            // Check if the stylesheet failed to load
            if (link.sheet === null) {
                var href = link.getAttribute('href');
                
                // Try different path variations
                var paths = [
                    href,
                    './' + href,
                    baseUrl + href.replace(/^\.?\/?/, ''),
                    window.location.pathname + href.replace(/^\.?\/?/, '')
                ];
                
                // Test each path until one works
                for (var i = 0; i < paths.length; i++) {
                    var testLink = document.createElement('link');
                    testLink.rel = 'stylesheet';
                    testLink.href = paths[i];
                    testLink.onload = function() {
                        // Remove the original broken link
                        if (link.parentNode) {
                            link.parentNode.removeChild(link);
                        }
                        console.log('Fixed stylesheet path: ' + this.href);
                    };
                    testLink.onerror = function() {
                        // Remove failed test link
                        if (this.parentNode) {
                            this.parentNode.removeChild(this);
                        }
                    };
                    document.head.appendChild(testLink);
                    break; // Try one at a time
                }
            }
        });
    }
    
    // Run check after DOM is loaded
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', checkStylesheets);
    } else {
        checkStylesheets();
    }
})();