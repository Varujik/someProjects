var enterN = prompt("Enter rows number",8);
var enterM = prompt("Enter columns number", 12);
if (!isNaN(enterN) && !isNaN(enterM) && enterN <=8 && enterM <= 50 && enterN >= 5 && enterM >= 5) {
    N = enterN;
    M = enterM;
}
else {
    var N = 8;
    var M = 8;
}
var snakeSpeed = 200; // Default speed
var route;
var beforesArrayI = []; // here we save coordinates, while moving body
var beforesArrayJ = []; // here the same
var headI; // coords of head (I)
var headJ; // coords of head (J)
var indexI;
var indexJ;
var lastPartDirection = "RIGHT"; // we need it while adding new part of body
var fruitTd;
var fruitImg;
var table = document.createElement("table");
document.body.appendChild(table);
// let's create 2d matrix
for (i = 0; i < N; i++) {
    var tr = document.createElement("tr");
    table.appendChild(tr);
    for (var j = 0; j < M; j++) {
        var td = document.createElement("td");
        if (((i + j) % 2) === 0) {
            td.style.backgroundColor = "green";
        }
        else {
            td.style.backgroundColor = "brown";
        }
        td.style.width = "100px";
        td.style.height = "100px";

        td.id = i + "" + j;
        table.appendChild(td);
        td.innerHTML = td.id;
    }
}
document.body.onkeyup = newRoute; // eventHandler
var partArray = new Array(1);
partArray[0] = {
    id: "04",
    image: undefined
};
/* creating head */
createHeadAndSomeBody();
while(!createNewFruit()) {}
//route = setInterval(goRight, snakeSpeed);

function createHeadAndSomeBody() {
    var headTd;
    var headImg;
    headTd = document.getElementById(0 + "" + 4);
    headImg = document.createElement("img");
    headImg.src = "img/" + "head.png";
    headImg.style.width = "100%";
    headImg.style.height = "60%";
    headTd.appendChild(headImg);
    partArray[0].image = headImg;

    for (var i = 1; i <= 2; i++) {
        var bodyTd;
        var bodyImg;
        partArray[i] = {
            id: "0" + (4- i),
            image: undefined
        };
        bodyTd = document.getElementById(0 + "" + (4 - i));
        bodyImg = document.createElement("img");
        bodyImg.src = "img/" + "body.png";
        bodyImg.style.width = "100%";
        bodyImg.style.height = "60%";
        bodyTd.appendChild(bodyImg);
        partArray[i].image = bodyImg;
    }
}

function goRight() { // to right
    headI = partArray[0].id[0];
    var headId = partArray[0].id;
    if (headId[2] !== undefined) {
        headJ = partArray[0].id[1] + partArray[0].id[2];
    }
    else {
        headJ = partArray[0].id[1];
    }
    indexI = parseInt(headI);
    indexJ = parseInt(headJ) + 1;
    // Moving head
    if (indexI >= 0 && indexJ >= 0 && indexI <= N - 1 && indexJ <= M - 1) {
        var nextLeftTd = document.getElementById(indexI + "" + indexJ);
        partArray[0].id = indexI + "" + indexJ;
        nextLeftTd.appendChild(partArray[0].image);
        // Moving all body
        moveBodys();
    }
    else {
        clearInterval(route);
        alert("You lose");
        location.reload();
    }
}
function goLeft() { // function to go to left
    headI = partArray[0].id[0];
    var headId = partArray[0].id;
    if (headId[2] !== undefined) {
        headJ = partArray[0].id[1] + partArray[0].id[2];
    }
    else {
        headJ = partArray[0].id[1];
    }
    indexI = parseInt(headI);
    indexJ = parseInt(headJ) - 1;
    if (indexI >= 0 && indexJ >= 0 && indexI <= N - 1 && indexJ <= M - 1) {
        var nextLeftTd = document.getElementById(indexI + "" + indexJ);
        partArray[0].id = indexI + "" + indexJ;
        nextLeftTd.appendChild(partArray[0].image);
        moveBodys();
    }
    else {
        clearInterval(route);
        alert("You lose");
        location.reload();
    }
}
function goTop() { // to top
    headI = partArray[0].id[0];
    var headId = partArray[0].id;
    if (headId[2] !== undefined) {
        headJ = partArray[0].id[1] + partArray[0].id[2];
    }
    else {
        headJ = partArray[0].id[1];
    }
    indexI = parseInt(headI) - 1;
    indexJ = parseInt(headJ);
    if (indexI >= 0 && indexJ >= 0 && indexI <= N - 1 && indexJ <= M - 1) {
        var nextLeftTd = document.getElementById(indexI + "" + indexJ);
        partArray[0].id = indexI + "" + indexJ;
        nextLeftTd.appendChild(partArray[0].image);
        moveBodys();
    }
    else {
        clearInterval(route);
        alert("You lose");
        location.reload();
    }
}
function goBottom() { // to bottom
    headI = partArray[0].id[0];
    var headId = partArray[0].id;
    if (headId[2] !== undefined) {
        headJ = partArray[0].id[1] + partArray[0].id[2];
    }
    else {
        headJ = partArray[0].id[1];
    }
    indexI = parseInt(headI) + 1;
    indexJ = parseInt(headJ);
    if (indexI >= 0 && indexJ >= 0 && indexI <= N - 1 && indexJ <= M - 1) {
        var nextLeftTd = document.getElementById(indexI + "" + indexJ);
        partArray[0].id = indexI + "" + indexJ;
        nextLeftTd.appendChild(partArray[0].image);
        moveBodys();
    }
    else {
        clearInterval(route);
        alert("You lose");
        location.reload();
    }
}
function moveBodys() { // moveBody considering head 
    for (var i = 1; i < partArray.length; i++) {
        if (i === 1) {
            beforesArrayI[i - 1] = partArray[i].id[0];
            if (partArray[i].id[2] !== undefined) {
                beforesArrayJ[i - 1] = partArray[i].id[1] + partArray[i].id[2];
            }
            else {
                beforesArrayJ[i - 1] = partArray[i].id[1];
            }
            nextLeftTd = document.getElementById(headI + "" + headJ);
            partArray[i].id = headI + "" + headJ;
            nextLeftTd.appendChild(partArray[i].image);
        }
        else {
            beforesArrayI[i - 1] = partArray[i].id[0];
            if (partArray[i].id[2] !== undefined) {
                beforesArrayJ[i - 1] = partArray[i].id[1] + partArray[i].id[2];
            }
            else {
                beforesArrayJ[i - 1] = partArray[i].id[1];
            }
            nextLeftTd = document.getElementById(beforesArrayI[i - 2] + "" + beforesArrayJ[i - 2]);
            partArray[i].id = beforesArrayI[i - 2] + "" + beforesArrayJ[i - 2];
            nextLeftTd.appendChild(partArray[i].image);
        }
    }
    for (i = 0; i < partArray.length; i++) {
        for (var j = 0; j < partArray.length; j++) {
            if (partArray[i].id === partArray[j].id && i !== j) {
                alert("You lose");
                location.reload();
                return false;
            }
        }
    }
    var differenceI = beforesArrayI[beforesArrayI.length - 2] - beforesArrayI[beforesArrayI.length - 1];
    var differenceJ = beforesArrayJ[beforesArrayJ.length - 2] - beforesArrayJ[beforesArrayJ.length - 1];
    if (differenceI > 0) {
        lastPartDirection = "BOTTOM";
    }
    else if (differenceI < 0) {
        lastPartDirection = "TOP";
    }
    else if (differenceJ > 0) {
        lastPartDirection = "RIGHT";
    }
    else if (differenceJ < 0) {
        lastPartDirection = "LEFT";
     }
    if (partArray[0].id === fruitTd.id) { // if head eats fruit
        fruitTd.removeChild(fruitImg);
        createNewBody();
        while(!createNewFruit())
        {}
    }
}
function createNewBody() { // create new body, considering direction of last part of body
    var bodyTd;
    var bodyImg;
    var I;
    var J;
    var lastBody = partArray[partArray.length - 1];
    switch (lastPartDirection) {
        case "RIGHT": {
            I = 0;
            J = -1;
        }
            break;
        case "LEFT": {
            I = 0;
            J = 1;
        }
            break;
        case "TOP": {
            I = 1;
            J = 0;
        }
            break;
        case "BOTTOM": {
            I = -1;
            J = 0;
        }
            break;
    }

    partArray[partArray.length] = {
        id: (parseInt(lastBody.id[0]) + parseInt(I)) + "" + (parseInt(lastBody.id[1] + lastBody.id[2]) + parseInt(J)),
        image: undefined
    };
    bodyTd = document.getElementById(partArray[partArray.length - 1].id);
    bodyImg = document.createElement("img");
    bodyImg.src = "img/" + "body.png";
    bodyImg.style.width = "100%";
    bodyImg.style.height = "60%";
    bodyTd.appendChild(bodyImg);
    partArray[partArray.length - 1].image = bodyImg;
}

function createNewFruit() { // randomly create new fruit
    var randomI = Math.floor(Math.random() * (N)); // [0-N]
    var randomJ = Math.floor(Math.random() * (M)); // [0-M]
    var newFruitId;
    var isValid = false;
    for (var i = 0; i < partArray.length; i++) {
        newFruitId = randomI + "" + randomJ;

        if (partArray[i].id !== newFruitId) {
            isValid = true;
        }
        else {
            isValid = false;
            break;
        }
    }
    if (isValid) {
        fruitTd = document.getElementById(newFruitId);
        fruitImg = document.createElement("img");
        fruitImg.src = "img/" + "apple.png";
        fruitImg.style.width = "100%";
        fruitImg.style.height = "60%";
        fruitTd.appendChild(fruitImg);
        return true;
    }
    return false;
}
function newRoute(e) { // where are we going ?
    if (e.keyCode === 39) { // right
        clearInterval(route);
        route = setInterval(goRight, snakeSpeed); 
    }
    if (e.keyCode === 37) { // left
        clearInterval(route);
        route = setInterval(goLeft, snakeSpeed); 
    }
    if (e.keyCode === 38) { // top
        clearInterval(route);
        route = setInterval(goTop, snakeSpeed); 
    }
    if (e.keyCode === 40) { // bottom
        clearInterval(route);
        route = setInterval(goBottom, snakeSpeed); 
    }
}

var inputSnakeSpeed = document.getElementById("snakeSpeed");
var snakeSpeedBtn = document.getElementById("snakeSpeedBtn");
snakeSpeedBtn.onclick = function() {
    var speed = parseInt(inputSnakeSpeed.value);
    if  (!isNaN(speed)) {
        snakeSpeed = speed;
    }
};