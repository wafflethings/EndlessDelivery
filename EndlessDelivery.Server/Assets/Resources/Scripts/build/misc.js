export function getParameterByName(name, url = window.location.href) {
    name = name.replace(/[\[\]]/g, '\\$&');
    let regex = new RegExp('[?&]' + name + '(=([^&#]*)|&|#|$)'), results = regex.exec(url);
    if (!results)
        return null;
    if (!results[2])
        return '';
    return decodeURIComponent(results[2].replace(/\+/g, ' '));
} // https://stackoverflow.com/questions/901115/how-can-i-get-query-string-values-in-javascript
// https://www.w3schools.com/js/js_cookies.asp
export function setCookie(cname, cvalue, exdays) {
    const d = new Date();
    d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
    let expires = "expires=" + d.toUTCString();
    document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
}
export function getCookie(cname) {
    let name = cname + "=";
    let ca = document.cookie.split(';');
    for (let i = 0; i < ca.length; i++) {
        let c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}
export function deleteCookie(cname) {
    setCookie(cname, "", -1);
}
let profilePictureTop = document.querySelector(".top-bar-pfp");
let miniProfileTop = document.querySelector(".top-mini-profile");
window.addEventListener('click', function (e) {
    if (miniProfileTop == null || e.target == null) {
        return;
    }
    if (!(miniProfileTop === null || miniProfileTop === void 0 ? void 0 : miniProfileTop.contains(e.target)) && !(profilePictureTop === null || profilePictureTop === void 0 ? void 0 : profilePictureTop.contains(e.target))) {
        miniProfileTop.style.visibility = "hidden";
    }
    else {
        miniProfileTop.style.visibility = "visible";
    }
});
let logOutMiniProfile = document.getElementById("log-out-link");
logOutMiniProfile === null || logOutMiniProfile === void 0 ? void 0 : logOutMiniProfile.addEventListener("pointerdown", function () {
    let xmlHttp = new XMLHttpRequest();
    xmlHttp.open("GET", "/api/users/clear_token", false);
    xmlHttp.send(null);
    deleteCookie("token");
    location.reload();
});
document.querySelectorAll(".hide-until-js").forEach(function (element) {
    if (element == null) {
        return;
    }
    element.animate([{ opacity: 0 }, { opacity: 1 }], { duration: 200, fill: "forwards", easing: "ease-in-out" });
    element.style.visibility = "visible";
});
