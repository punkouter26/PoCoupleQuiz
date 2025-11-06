// Haptic feedback for mobile devices
window.triggerHapticFeedback = function() {
    if ('vibrate' in navigator) {
        navigator.vibrate([50, 30, 50]); // Short-pause-short pattern
    }
};

// Success haptic (stronger)
window.triggerSuccessHaptic = function() {
    if ('vibrate' in navigator) {
        navigator.vibrate([100, 50, 100, 50, 100]); // Triple pulse
    }
};

// Light tap haptic
window.triggerLightHaptic = function() {
    if ('vibrate' in navigator) {
        navigator.vibrate(20); // Single short pulse
    }
};
