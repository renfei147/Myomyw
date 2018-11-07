/*继承后重载下面的函数:
onBeganMoving(col, last) 开始移动后调用,col为移动列号,last为移出去的球
onEndedMoving(col, last) 结束移动后调用,col为移动列号,last为移出去的球
onChangedTurn() 切换回合后调用
onWin(timeout) 胜利后调用 胜利方为this.turn的反方 timeout表示是否由于超时
*/
class GameScene extends cc.Scene {
    /*
    leftName 左边玩家的名字
    rightName 右边玩家的名字
    controllableSide 可控制的方向(left, right, both, neither)
    createNextChessman 设置获取下一个棋子的函数(设为null则需要手动调用setNextChessman)
    enableTimer 是否开启限时
    */
    constructor(leftName, rightName, controllableSide, createNextChessman, enableTimer) {
        super();
        this.chessmen = [];
        for (let i = 0; i < maxLCol; i++) {
            this.chessmen[i] = [];
            for (let j = 0; j < maxRCol; j++) {
                this.chessmen[i][j] = Chessman.common;
            }
        }
        this.lCol = defaultLCol;
        this.rCol = defaultRCol;
        this.nextChessman = null;
        this.turn = null;
        this.playing = false;
        this.totalMovements = null;
        this.action = Action.nothing;
        this.moveByTouching = false;
        this.touching = false ;
            //TID = Timeout ID
        this.coolTID = null;
        this.timerTID = null;
        this.handleMovingEndTID = null;
            //TFN = Timeout function
        this.handleMovingEndTFN = null;

        this.halfDiagonal = null;
        this.diagonal = null;
        this.topVertX = null;  //棋盘上面的顶点的x坐标(这可以表示很多东西)

        const backButton = new ccui.Button(res.BackButtonN_png, res.BackButtonS_png);
        backButton.setPosition(backButton.width / 2 + 10, backButton.height / 2 + 10);
        backButton.addClickEventListener(function () {
            cc.director.runScene(new MainScene());
        });
        this.addChild(backButton, 10);

        this.boardLength = Math.min(size.width, size.height) - 10;
        this.board = new cc.Layer();
        this.board.ignoreAnchorPointForPosition(false);
        this.board.attr({
            width: this.boardLength,
            height: this.boardLength,
            x: size.width / 2,
            y: size.height / 2
        });
        this.addChild(this.board);

        this.gridNode = new cc.Node();
        this.board.addChild(this.gridNode, 0);
        this.border = new cc.DrawNode();
        this.board.addChild(this.border, 2);

        this.stencilDrawNode = new cc.DrawNode();
        this.stencil = new cc.ClippingNode(this.stencilDrawNode);
        this.board.addChild(this.stencil, 1);
        this.chessmanNode = new cc.Node();
        this.stencil.addChild(this.chessmanNode);

        this.highlightingCol = null;
        this.highlightDrawNode = new cc.DrawNode();
        this.highlightDrawNode.setPosition(0, 0);
        this.board.addChild(this.highlightDrawNode, 3);

        this.createNextChessman = createNextChessman;
        this.controllableSide = controllableSide;
        this.enableTimer = enableTimer || false;
        if (this.enableTimer) {
            this.timerStencilDrawNode = new cc.DrawNode();
            this.timerStencil = new cc.ClippingNode(this.timerStencilDrawNode);
            this.board.addChild(this.timerStencil);

            this.timer = new cc.LayerColor(cc.color(255, 255, 255, 100));
            this.timer.ignoreAnchorPointForPosition(false);
            this.timer.setAnchorPoint(0.5, 1);
            this.timerStencil.addChild(this.timer);
        }
        else {
            this.timerStencilDrawNode = null;
            this.timerStencil = null;
            this.timer = null;
        }

        this.leftNameLabel = creator.createLabel(leftName, 25);
        this.leftNameLabel.setPosition(this.leftNameLabel.width / 2 + 20, size.height - this.leftNameLabel.height / 2 - 20);
        this.addChild(this.leftNameLabel);
        this.rightNameLabel = creator.createLabel(rightName, 25);
        this.rightNameLabel.setPosition(size.width - this.rightNameLabel.width / 2 - 20, size.height - this.rightNameLabel.height / 2 - 20);
        this.addChild(this.rightNameLabel);

        const touchEvent = {
            event: cc.EventListener.TOUCH_ONE_BY_ONE,
            onTouchBegan: this.ejectorTouchBegan.bind(this),
            onTouchEnded: this.ejectorTouchEnded.bind(this)
        };

        if ("touches" in cc.sys.capabilities) {
            touchEvent.onTouchMoved = this.ejectorTouchMoved.bind(this);
        }
        cc.eventManager.addListener(touchEvent, this.board);

        cc.eventManager.addListener({
            event: cc.EventListener.MOUSE,
            onMouseMove: this.onMouseMove.bind(this)
        }, this.board);

        this.buildChessboard();
    }

    ejectorTouchBegan(touch, event) {
        if (this.isControllable() && this.playing && this.action !== Action.moving) {
            const point = this.board.convertTouchToNodeSpace(touch);
            const ejector = this.getEjectorByPoint(point);
            if (ejector == null) {
                this.touchingForHighlight = true;
            }
            else {
                this.moveByTouching = true;
                this.touching = true;
                this.highlightCol(null);
                this.move(ejector);
            }
            return true;
        }
        return false;
    }

    ejectorTouchEnded(touch, event) {
        if (this.touching) {
            this.touching = false;
            if (this.action === Action.cooling) {
                clearTimeout(this.coolTID);
                this.changeTurn();
            }
        }
        this.touchingForHighlight = false;
        if (this.playing){
            this.highlightCol(null);
        }
    }

    ejectorTouchMoved(touch, event) {
        if (this.touchingForHighlight && this.playing) {
            const point = this.board.convertTouchToNodeSpace(touch);
            const ejector = this.getEjectorByPoint(point);
            this.highlightCol(ejector);
        }
    }

    onMouseMove(event) {
        if (this.isControllable() && this.playing && this.action === Action.nothing) {
            const point = this.board.convertToNodeSpace(event.getLocation());
            const ejector = this.getEjectorByPoint(point);
            this.highlightCol(ejector);
        }
    }

    //输入null则清除高亮
    highlightCol(col) {
        if (col !== this.highlightingCol) {
            this.highlightingCol = col;
            this.highlightDrawNode.clear();
            if (col != null) {
                if (this.turn === left) {
                    var poly = [
                        cc.p(this.topVertX - this.halfDiagonal * col,
                            this.boardLength - this.diagonal - this.halfDiagonal * col),
                        cc.p(this.topVertX - this.halfDiagonal * (col + 1),
                            this.boardLength - this.diagonal - this.halfDiagonal * (col + 1)),
                        cc.p(this.boardLength - this.diagonal - this.halfDiagonal * col,
                            this.topVertX - this.diagonal - this.halfDiagonal * col),
                        cc.p(this.boardLength - this.halfDiagonal * (col + 1),
                            this.topVertX - this.halfDiagonal * (col + 1))
                    ];
                    var color = cc.color(0, 255, 0, 50);
                }
                else {
                    var poly = [
                        cc.p(this.topVertX + this.halfDiagonal * col,
                            this.boardLength - this.diagonal - this.halfDiagonal * col),
                        cc.p(this.topVertX + this.halfDiagonal * (col + 1),
                            this.boardLength - this.diagonal - this.halfDiagonal * (col + 1)),
                        cc.p(this.diagonal + this.halfDiagonal * col,
                            this.boardLength - this.topVertX - this.diagonal - this.halfDiagonal * col),
                        cc.p(this.halfDiagonal * (col + 1),
                            this.boardLength - this.topVertX - this.halfDiagonal * (col + 1))
                    ];
                    var color = cc.color(0, 100, 255, 50);
                }
                this.highlightDrawNode.drawPoly(poly, color, 0, color);
            }
        }
    }

    getEjectorByPoint(point) {
        for (let i = 0; i < (this.turn === left ? this.lCol : this.rCol) ; i++) {
            let ejector = this.gridNode.getChildByTag(this.turn === left ? i : this.lCol + i);
            if (!ejector) continue;
            const rpoint = cc.p(point.x - ejector.x, point.y - ejector.y);
            if (rpoint.x + rpoint.y < this.halfDiagonal &&
                rpoint.x + rpoint.y > -this.halfDiagonal &&
                rpoint.x - rpoint.y > -this.halfDiagonal &&
                rpoint.y - rpoint.x > -this.halfDiagonal) {
                return i;
            }
        }
        return null;
    }

    isControllable() {
        return this.controllableSide === both || this.controllableSide === this.turn;
    }

    start(side) {
        this.playing = true;
        this.setTurn(side);
        if (this.createNextChessman) {
            this.setNextChessman(this.createNextChessman());
        }
        if (this.enableTimer) {
            this.startTimer();
        }
    }

    setNextChessman(chessman) {
        this.nextChessman = chessman;
        const moveAction = cc.moveBy(0.5, cc.p(0, -20));
        const old = this.getChildByName("next");
        if (old) {
            old.setName("");
            const fadeOutAction = cc.fadeOut(0.5);
            old.runAction(moveAction);
            old.runAction(fadeOutAction);
            setTimeout(this.removeChild.bind(this, old), 500);
        }

        const nextChessmanSprite = this.createSpriteByChessman(this.nextChessman);
        nextChessmanSprite.attr({
            opacity: 0,
            scale: 0.8,
            x: size.width - 50,
            y: size.height - 80
        });
        nextChessmanSprite.setName("next");
        const fadeInAction = cc.fadeIn(0.5);
        nextChessmanSprite.runAction(moveAction.clone());
        nextChessmanSprite.runAction(fadeInAction);
        this.addChild(nextChessmanSprite);
    }

    createSpriteByChessman(type) {
        const chessman = new cc.Sprite(chessmanTex[type]);
        chessman.setScaleX(this.halfDiagonal / chessman.width);
        chessman.setScaleY(this.halfDiagonal / chessman.height);
        return chessman;
    }

    buildChessboard() {
        const drawLCol = this.lCol + 1, drawRCol = this.rCol + 1; //画图时的网格数量(加上发射器)
        this.halfDiagonal = this.boardLength / (drawLCol + drawRCol);
        this.diagonal = 2 * this.halfDiagonal;
        this.topVertX = drawLCol * this.halfDiagonal;
        this.stencilDrawNode.clear();

        //网格(包括背景网格和发射器)
        this.gridNode.removeAllChildren();
        let ejectorScale; //所有Grid目录下的贴图应该有相同的分辨率,且长宽相同
        for (let l = 0; l < drawLCol; l++) {
            for (let r = 0; r < drawRCol; r++) {
                let grid;
                if (l === 0 && r === 0) {
                    continue;
                }
                else if (r === 0) {
                    grid = new cc.Sprite(res.GreenEjector_png);
                    grid.tag = l - 1;
                }
                else if (l === 0) {
                    grid = new cc.Sprite(res.BlueEjector_png);
                    grid.tag = this.lCol + r - 1;
                }
                else if ((l + r) % 2 === 0) {
                    grid = new cc.Sprite(res.Grid1_png);
                }
                else {
                    grid = new cc.Sprite(res.Grid2_png);
                }

                if (!ejectorScale) {
                    ejectorScale = this.diagonal / Math.sqrt(2 * grid.width * grid.width);
                }
                grid.attr({
                    rotation: 45,
                    scale: ejectorScale,
                    x: (r - l) * this.halfDiagonal + this.topVertX,
                    y: this.boardLength - (l + r + 1) * this.halfDiagonal
                });
                this.gridNode.addChild(grid);
            }
        }
        //边框线
        this.border.clear();
        for (let i = 0; i <= drawLCol; i++) {
            this.border.drawSegment(cc.p(this.topVertX - this.halfDiagonal * i, this.boardLength - this.halfDiagonal * i),
                i > 1 && i < drawLCol ?
                cc.p(this.topVertX - this.halfDiagonal * (i - 1), this.boardLength - this.halfDiagonal * (i + 1)) :
                cc.p(this.topVertX - this.halfDiagonal * (i - drawRCol), this.boardLength - this.halfDiagonal * (i + drawRCol)),
                1, cc.color(128, 128, 128));
        }
        for (let i = 0; i <= drawRCol; i++) {
            this.border.drawSegment(cc.p(this.topVertX + this.halfDiagonal * i, this.boardLength - this.halfDiagonal * i),
                i > 1 && i < drawRCol ?
                cc.p(this.topVertX + this.halfDiagonal * (i - 1), this.boardLength - this.halfDiagonal * (i + 1)) :
                cc.p(this.topVertX + this.halfDiagonal * (i - drawLCol), this.boardLength - this.halfDiagonal * (i + drawLCol)),
               1, cc.color(128, 128, 128));
        }

        if (this.enableTimer) {
            this.timerStencilDrawNode.clear();
            const timerStencilPoly = [
                cc.p(this.topVertX, this.boardLength),
                cc.p(this.topVertX - this.halfDiagonal, this.boardLength - this.halfDiagonal),
                cc.p(this.topVertX, this.boardLength - this.diagonal),
                cc.p(this.topVertX + this.halfDiagonal, this.boardLength - this.halfDiagonal)
            ];
            this.timerStencilDrawNode.drawPoly(timerStencilPoly, cc.color(0, 0, 0, 0), 0, cc.color(0, 0, 0, 0));
        }

        //遮罩不包括发射器部分
        const stencilPoly = [
            cc.p(this.topVertX, this.boardLength - this.diagonal),
            cc.p(this.halfDiagonal, this.boardLength - this.topVertX - this.halfDiagonal),
            cc.p(this.boardLength - this.topVertX, 0),
            cc.p(this.boardLength - this.halfDiagonal, this.topVertX - this.halfDiagonal)
        ];
        this.stencilDrawNode.drawPoly(stencilPoly, cc.color(0, 0, 0, 0), 0, cc.color(0, 0, 0, 0));
        this.updateChessboard();
        this.setTurnFlag();
    }

    updateChessboard() {
        this.chessmanNode.removeAllChildren();
        for (let l = 0; l < this.lCol; l++) {
            for (let r = 0; r < this.rCol; r++) {
                const chessman = this.createSpriteByChessman(this.chessmen[l][r]);
                chessman.setPosition((r - l) * this.halfDiagonal + this.topVertX,
                    this.boardLength - (l + r + 3) * this.halfDiagonal);
                chessman.setTag(l * this.rCol + r);
                this.chessmanNode.addChild(chessman);
            }
        }
    }

    setTurnFlag() {
        const leftOpacity = this.turn === left ? 255 : 150;
        const rightOpacity = this.turn === right ? 255 : 150;
        for (let i = 0; i < this.lCol; i++) {
            this.gridNode.getChildByTag(i).setOpacity(leftOpacity);
        }
        for (let i = 0; i < this.rCol; i++) {
            this.gridNode.getChildByTag(this.lCol + i).setOpacity(rightOpacity);
        }

        this.leftNameLabel.color = this.turn === left ? cc.color(0, 200, 0) : cc.color(0, 0, 0);
        this.rightNameLabel.color = this.turn === right ? cc.color(0, 0, 200) : cc.color(0, 0, 0);

        if (this.turn != null) {
            const scaleAction = cc.scaleTo(0.2, 1.2);
            const resetAction = cc.scaleTo(0.2, 1.0);
            this.leftNameLabel.runAction(this.turn === left ? scaleAction : resetAction);
            this.rightNameLabel.runAction(this.turn === right ? scaleAction : resetAction);
            if (this.enableTimer) {
                this.timer.color = this.turn === left ? cc.color(0, 255, 0) : cc.color(0, 100, 255);
            }
        }
    }

    setTurn(turn) {
        this.turn = turn;
        this.totalMovements = 0;
        this.touching = false;
        this.moveByTouching = false;
        this.action = Action.nothing;
        this.setTurnFlag();
    }

    changeTurn() {
        this.setTurn(this.turn === left ? right : left);
        if (this.enableTimer) {
            this.startTimer();
        }
        this.onChangedTurn();
    }

    move(col) {
        if (this.playing && this.action !== Action.moving) {
            if (this.enableTimer) {
                this.stopTimer();
            }
            this.action = Action.moving;
            this.totalMovements++;
            let lastChessman; //暂存最底下的棋子
            if (this.turn === left) {
                lastChessman = this.chessmen[col][this.rCol - 1];
                for (let i = this.rCol - 1; i > 0; i--) {
                    this.chessmen[col][i] = this.chessmen[col][i - 1];
                }
                this.chessmen[col][0] = this.nextChessman;
            }
            else {
                lastChessman = this.chessmen[this.lCol - 1][col];
                for (let i = this.lCol - 1; i > 0; i--) {
                    this.chessmen[i][col] = this.chessmen[i - 1][col];
                }
                this.chessmen[0][col] = this.nextChessman;
            }

            //移动动画
            const newChessman = this.createSpriteByChessman(this.nextChessman);
            newChessman.setPosition(
                this.halfDiagonal * (this.turn === left ? -col - 1 : col + 1) + this.topVertX,
                this.boardLength - this.halfDiagonal * (col + 2));
            this.chessmanNode.addChild(newChessman);
            const movingAction = cc.moveBy(movingTime,
                cc.p(this.turn === left ? this.halfDiagonal : -this.halfDiagonal, -this.halfDiagonal));
            newChessman.runAction(movingAction);
            for (let i = 0; i < (this.turn === left ? this.rCol : this.lCol) ; i++) {
                this.chessmanNode.getChildByTag(
                    this.turn === left ? col * this.rCol + i : i * this.rCol + col).runAction(movingAction.clone());
            }

            const func = this.handleMovingEnd.bind(this, col, lastChessman);
            this.handleMovingEndTFN = func;
            this.handleMovingEndTID = setTimeout(func, movingTime * 1000);

            if (this.createNextChessman) {
                this.setNextChessman(this.createNextChessman());
            }
            this.onBeganMoving(col, lastChessman);
        }
    }

    coolAndMove(col) {
        this.coolTID = setTimeout(this.move.bind(this, col), coolingTime * 1000);
    }

    //移动结束后的处理
    handleMovingEnd(col, lastChessman) {
        switch (lastChessman) {
            case Chessman.key:
                this.updateChessboard();
                this.playing = false;
                this.onWin(false);
                return;
            case Chessman.addCol:
                if (this.turn === left)
                    this.setBoardSize(this.lCol, this.rCol + 1);
                else
                    this.setBoardSize(this.lCol + 1, this.rCol);
                break;
            case Chessman.delCol:
                if (this.turn === left)
                    this.setBoardSize(this.lCol, this.rCol - 1);
                else
                    this.setBoardSize(this.lCol - 1, this.rCol);
                break;
            case Chessman.flip:
                this.flip();
                this.changeTurn();
                break;
            default:
                this.updateChessboard();
                break;
        }
        this.onEndedMoving(col, lastChessman);
        if (this.moveByTouching) {
            if (this.touching && this.totalMovements < maxMovements) {
                this.coolAndMove(col);
                this.action = Action.cooling;
            }
            else {
                this.changeTurn();
            }
        }
        else {
            this.action = Action.nothing;
        }
    }

    endMovingAtOnce() {
        const allChessmen = this.chessmanNode.getChildren();
        for (let i = 0; i < allChessmen.length; i++) {
            allChessmen[i].stopAllActions();
        }
        clearTimeout(this.handleMovingEndTID);
        this.handleMovingEndTFN();
        this.handleMovingEndTFN = null;
    }

    setBoardSize(lCol, rCol) {
        if (lCol <= maxLCol && lCol >= minLCol && rCol <= maxRCol && rCol >= minRCol) {
            //如果棋盘变大，把多出来的部分设置为普通球
            if (this.lCol < lCol) {
                for (let l = this.lCol; l < lCol; l++) {
                    for (let r = 0; r < rCol; r++) {
                        this.chessmen[l][r] = Chessman.common;
                    }
                }
            }
            if (this.rCol < rCol) {
                for (let l = 0; l < lCol; l++) {
                    for (let r = this.rCol; r < rCol; r++) {
                        this.chessmen[l][r] = Chessman.common;
                    }
                }
            }

            this.board.setScale((lCol + rCol) / (this.lCol + this.rCol));
            const scaleAction = cc.scaleTo(scalingTime, 1);
            this.board.runAction(scaleAction);
            this.lCol = lCol;
            this.rCol = rCol;
            this.buildChessboard();
        }
        else {
            this.updateChessboard();
        }
    }

    flip() {
        for (let l = 0; l < maxLCol; l++) {
            for (let r = l + 1; r < maxRCol; r++) {
                this.chessmen[l][r] ^= this.chessmen[r][l];
                this.chessmen[r][l] ^= this.chessmen[l][r];
                this.chessmen[l][r] ^= this.chessmen[r][l];
            }
        }
        this.lCol ^= this.rCol;
        this.rCol ^= this.lCol;
        this.lCol ^= this.rCol;

        const scaleAction1 = cc.scaleTo(flippingTime / 2, 0, 1);
        const callAction = cc.callFunc(this.buildChessboard.bind(this));
        const scaleAction2 = cc.scaleTo(flippingTime / 2, 1);
        const sequenceAction = cc.sequence(scaleAction1, callAction, scaleAction2);
        this.board.runAction(sequenceAction);
    }

    startTimer() {
        this.stopTimer();
        this.timer.setPosition(this.topVertX, this.boardLength);
        const moveAction = cc.moveBy(timeLimit, cc.p(0, -this.diagonal));
        this.timer.runAction(moveAction);
        this.timerTID = setTimeout(function () {
            this.playing = false;
            this.onWin(true);
        }.bind(this), timeLimit * 1000);
    }

    stopTimer() {
        this.timer.stopAllActions();
        clearTimeout(this.timerTID);
    }

    onBeganMoving(col, last) { }

    onEndedMoving(col, last) { }

    onChangedTurn() { }

    onWin(timeout) { }
}
