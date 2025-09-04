window.requestNotificationPermission = async function () {
    if ("Notification" in window) {
        const permission = await Notification.requestPermission();
        return permission;
    }
};

window.getNotificationPermission = () => {
    if ("Notification" in window) {
        return Notification.permission;
    }
    return "denied";
};

window.showNotification = function (title, body) {
    if ("Notification" in window && Notification.permission === "granted") {
        const notification = new Notification(title, {
            body: body,
            icon: "/icon-64.png",
            requireInteraction: false,
            silent: false
        });

        setTimeout(() => {
            notification.close();
        }, 7000);
    }
};

