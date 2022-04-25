function clickCopy(button) {
    var value = button
        .parentElement
        .parentElement
        .getElementsByClassName('value')[0].innerText;
    console.log(value);
    navigator.clipboard.writeText(value);
}