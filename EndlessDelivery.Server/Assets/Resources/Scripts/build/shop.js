"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
document.querySelectorAll(".item-card").forEach(function (element) {
    element.addEventListener("pointerdown", function () {
        let itemId = element.getAttribute("item-id");
        if (itemId != null) {
            buyItem(itemId);
        }
    });
});
function buyItem(itemId) {
    return __awaiter(this, void 0, void 0, function* () {
        const response = yield fetch("/api/users/items/buy_item", {
            method: 'POST',
            body: "item_id=" + itemId,
            headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' }
        });
    });
}
