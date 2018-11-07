const maxLCol = 10, maxRCol = 10;
const minLCol = 3, minRCol = 3;
const defaultLCol = 6, defaultRCol = 6;
const maxMovements = 5;

//均以秒为单位
const movingTime = 0.3;
const coolingTime = 0.4;
const timeLimit = 20;
const scalingTime = 0.2;
const flippingTime = 0.4;
const aiThinkingTime = 0.8;

const Chessman = {common: 0, key: 1, addCol: 2, delCol: 3, flip: 4};
const chessmanTex = [res.Common_png, res.Key_png, res.AddCol_png, res.DelCol_png, res.Flip_png];
const left = 0, right = 1, both = 2, neither = 3;
const Action = {nothing: 0, moving: 1, cooling: 2};
const EndReason = {opponentLeft: 0, youWin: 1, opponentWins: 2, youOutOfTime: 3, opponentOutOfTime: 4, serverFull: 5};

function getRandomChessman() {
    switch (Math.floor(Math.random() * 11)) {
        case 0:
            return Chessman.key;
        case 1:
            return Chessman.addCol;
        case 2:
            return Chessman.delCol;
        case 3:
            return Chessman.flip;
        default:
            return Chessman.common;
    }
}

//cc.formatStr并不能代替这个
function format(str) {
    if (cc.isString(str)) {
        for (let i = 1; i < arguments.length; i++) {
            str = str.replace("{" + (i - 1) + "}", arguments[i]);
        }
    } else {
        str = "";
    }
    return str;
}
