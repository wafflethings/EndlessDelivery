import { getParameterByName } from "./misc.js";
let leaderboardBox = document.querySelector(".leaderboard-box"); // this is the one with all the LB entries as children
let elements = document.querySelectorAll(".score-box");
elements.forEach(function (element) {
    let primary = element.querySelector(".leaderboard-primary-content");
    ;
    let secondary = element.querySelector(".leaderboard-secondary-content");
    if (primary == null || secondary == null) {
        return;
    }
    secondary.setAttribute("start-height", secondary.offsetHeight.toString());
    secondary.setAttribute("enabled", "false");
    secondary.style.visibility = "hidden";
    secondary.style.maxHeight = "0";
    primary.addEventListener("pointerdown", function () {
        var _a;
        if (secondary == null) {
            return;
        }
        let goingVisible = secondary.getAttribute("enabled") == "false";
        secondary.setAttribute("enabled", goingVisible ? "true" : "false");
        let targetHeight = goingVisible ? (_a = secondary.getAttribute("start-height")) !== null && _a !== void 0 ? _a : "0" : "0";
        secondary.animate({ maxHeight: targetHeight + "px" }, { duration: 250, fill: "forwards", easing: "ease-out" });
        if (goingVisible) {
            secondary.style.visibility = "visible";
        }
        else {
            setTimeout(function () {
                if (secondary == null || secondary.getAttribute("enabled") == "true") {
                    return;
                }
                secondary.style.visibility = "hidden";
            }, 250);
        }
    });
});
let isValidToken = getParameterByName("token_success");
if (isValidToken != null) {
    if (isValidToken == "False") {
        alert("Token login failure.");
    }
}
