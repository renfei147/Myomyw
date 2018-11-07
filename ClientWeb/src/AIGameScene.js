class AIGameScene extends GameScene {
    //null代表不在移动,0代表刚移动完
    constructor() {
        super(player.name, txt.names.ai, left, getRandomChessman, storage.getItem("standaloneTimer") !== "false");
        this.aiMovements = null;
        this.start(left);
    }

    onBeganMoving(col, last) {
        if (last === Chessman.flip) {
            this.aiMovements = null;
        }
    }

    onEndedMoving(col, last) {
        if (this.aiMovements > 0) {
            this.aiMovements--;
            this.coolAndMove(col);
        }
        else if (this.aiMovements === 0) {
            this.changeTurn();
            this.aiMovements = null;
        }
    }

    onChangedTurn() {
        if (this.turn === right) {
            //切换回合后冷却一下再让AI下(否则看起来太突然)
            setTimeout(this.aiMove.bind(this), aiThinkingTime * 1000);
        }
    }

    aiMove() {
        let maxWeighting = -100;
        let bestCol = 0;
        for (let r = 0; r < this.rCol; r++) {
            let weighting = 0;
            for (let l = 0; l < this.lCol; l++) {
                let change = 0;
                switch (this.chessmen[l][r]) {
                    case Chessman.common:
                        change = 1;
                        break;
                    case Chessman.key:
                        //对最下面一列的红球给予特别特别关注
                        if (l === this.lCol - 1)
                            change = -10;
                        else
                            change = -3;
                        break;
                    case Chessman.addCol:
                        change = -1;
                        break;
                    case Chessman.delCol:
                        change = 2;
                        break;
                    case Chessman.filp:
                        if (this.lCol > this.rCol)
                            change = 1;
                        else
                            change = -1;
                }
                //离出口越近权重越大
                weighting += change * ((l + 1) / this.lCol);
            }
            if (weighting > maxWeighting) {
                maxWeighting = weighting;
                bestCol = r;
            }
        }

        //移动次数根据权重而变
        const times = Math.round(maxWeighting);
        //确定实际移动次数
        if (times < 1) {
            this.aiMovements = 1;
        }
        else if (times > maxMovements) {
            this.aiMovements = maxMovements;
        }
        else {
            this.aiMovements = times;
        }
        this.move(bestCol);
        this.aiMovements--;
    }

    onWin(timeout) {
        let str = "";
        if (timeout) {
            if (this.turn === left)
                str = txt.result.youOutOfTime;
            else
                str = txt.result.aiOutOfTime;
            str += "\n";
        }
        if (this.turn === left)
            str += txt.result.aiWins;
        else
            str += txt.result.youWin;

        this.addChild(new ResultLayer(str, cc.color(0, 0, 0)));
    }
}
