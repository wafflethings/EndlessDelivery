﻿document.querySelectorAll(".item-card").forEach(function (element : Element) : void {
    element.addEventListener("pointerdown", function () : void {
        let itemId : string | null = element.getAttribute("item-id");
        let unownedText : HTMLDivElement | null = element.querySelector("#unowned-price-text");
        let ownedText : HTMLDivElement | null = element.querySelector("#owned-price-text");

        if (itemId != null) {
            buyItem(itemId).then(s => {
                if (s != 200) { //200: OK
                    return;
                }

                if (unownedText != null) {
                    unownedText.style.display = "none";
                }

                if (ownedText != null) {
                    ownedText.style.display = "block";
                }

                updateStarCounter().then(r => r)
            });
        }
    });
});

async function updateStarCounter() {
    let counter = document.getElementById("star-counter");
    if (counter == null) {
        return;
    }
    const response : Response = await fetch("/api/users/get_currency_amount", {
        method: 'GET',
    });
    counter.innerText = await response.text();
}

async function buyItem(itemId : string) : Promise<number> {
    const response : Response = await fetch("/api/users/items/buy_item", {
        method: 'POST',
        body: itemId,
    });

    return response.status;
}
