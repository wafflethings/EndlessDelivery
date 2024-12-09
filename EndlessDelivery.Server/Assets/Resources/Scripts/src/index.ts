import { getParameterByName, deleteCookie } from "./misc.js";

let leaderboardBox : HTMLDivElement | null = document.querySelector(".leaderboard-box"); // this is the one with all the LB entries as children
let elements : NodeListOf<Element> = document.querySelectorAll(".score-box")

elements.forEach(function (element : Element) : void {
    let primary : HTMLDivElement | null = element.querySelector(".leaderboard-primary-content");;
    let secondary : HTMLDivElement | null = element.querySelector(".leaderboard-secondary-content");

    if (primary == null || secondary == null) {
        return;
    }

    secondary.setAttribute("start-height", secondary.offsetHeight.toString());
    secondary.setAttribute("enabled", "false");
    secondary.style.visibility = "hidden";
    secondary.style.maxHeight = "0";

    primary.addEventListener("pointerup", function () : void {
        if (secondary == null) {
            return;
        }

        let goingVisible : boolean  = secondary.getAttribute("enabled") == "false";
        secondary.setAttribute("enabled", goingVisible ? "true" : "false");

        let targetHeight : string = goingVisible ? secondary.getAttribute("start-height") ?? "0" : "0";
        secondary.animate({ maxHeight: targetHeight + "px" }, { duration: 250, fill: "forwards", easing: "ease-out" });

        if (goingVisible) {
            secondary.style.visibility = "visible";
        } else {
            setTimeout(function () : void {
                if (secondary == null || secondary.getAttribute("enabled") == "true") {
                    return;
                }
                secondary.style.visibility = "hidden";
            }, 250);
        }
    });
});

let isValidToken : string | null = getParameterByName("token_success")

if (isValidToken != null) {
    if (isValidToken == "False") {
        alert("Token login failure.")
    }
}
