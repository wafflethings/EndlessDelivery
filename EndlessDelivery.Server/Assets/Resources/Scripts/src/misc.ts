export function getParameterByName(name : string, url : string = window.location.href) : string | null {
    name = name.replace(/[\[\]]/g, '\\$&');
    let regex : RegExp = new RegExp('[?&]' + name + '(=([^&#]*)|&|#|$)'), results : RegExpExecArray | null = regex.exec(url);
    if (!results) return null;
    if (!results[2]) return '';
    return decodeURIComponent(results[2].replace(/\+/g, ' '));
} // https://stackoverflow.com/questions/901115/how-can-i-get-query-string-values-in-javascript

// https://www.w3schools.com/js/js_cookies.asp

export function setCookie(cname: string, cvalue: string, exdays: number) : void {
    const d = new Date();
    d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
    let expires = "expires="+d.toUTCString();
    document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
}

export function getCookie(cname: string) : string {
    let name = cname + "=";
    let ca = document.cookie.split(';');
    for(let i = 0; i < ca.length; i++) {
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

export function deleteCookie(cname : string) : void {
    setCookie(cname, "", -1)
}

let profilePictureTop : HTMLDivElement | null = document.querySelector(".top-bar-pfp");
let miniProfileTop : HTMLDivElement | null =  document.querySelector(".top-mini-profile");

window.addEventListener('click', function(e : MouseEvent) : void {
    if (miniProfileTop == null || e.target == null) {
        return;
    }
    if (!miniProfileTop?.contains(e.target as Node) && !profilePictureTop?.contains(e.target as Node)) {
        miniProfileTop.style.visibility = "hidden";
    }
    else {
        miniProfileTop.style.visibility = "visible";
    }
});

let logOutMiniProfile : HTMLElement | null =  document.getElementById("log-out-link");

logOutMiniProfile?.addEventListener("pointerdown", function () : void {
    let xmlHttp : XMLHttpRequest = new XMLHttpRequest();
    xmlHttp.open("GET", "/api/users/clear_token", false);
    xmlHttp.send(null);
    deleteCookie("token");
    location.reload();
});

window.onload = function() : void {
    document.querySelectorAll(".scale-font-to-fit").forEach(function (element : Element) : void {
        const htmlElement : HTMLElement = element as HTMLElement;
        resizeToFit(htmlElement);

        window.addEventListener("resize", () : void => {
            resizeToFit(htmlElement);
        });
    });
}

function resizeToFit(element : HTMLElement) : void {
    if (element.parentElement == null) {
        return;
    }

    if (element.clientHeight - 2 > parseFloat(window.getComputedStyle(element).lineHeight)) {
        console.log(element.clientWidth + ">" + parseFloat(window.getComputedStyle(element).lineHeight))
        element.style.fontSize = (parseFloat(window.getComputedStyle(element).fontSize) - 1) + 'px';
        resizeToFit(element)
    }

    if (element.clientHeight + 2 < parseFloat(window.getComputedStyle(element).lineHeight)) {
        console.log(element.clientWidth + ">" + parseFloat(window.getComputedStyle(element).lineHeight))
        element.style.fontSize = (parseFloat(window.getComputedStyle(element).fontSize) + 1) + 'px';
        resizeToFit(element)
    }
}

document.querySelectorAll(".hide-until-js").forEach(function (element : Element) : void {
    if (element == null) {
        return;
    }

    element.animate([{ opacity: 0 }, { opacity: 1 }], { duration: 200, fill: "forwards", easing: "ease-in-out" });
    (element as HTMLElement).style.visibility = "visible";
})
