class TutorialGameScene extends GameScene {
    constructor() {
        let chessGen = new class {
            constructor() {
                this.firstNewChessman = true;
            }
            gen() {
                if (this.firstNewChessman) {
                    this.firstNewChessman = false;
                    return Chessman.key;
                }
                else {
                    return Chessman.common;
                }
            }
        };
        super(player.name, txt.names.ai, left, chessGen.gen.bind(chessGen), true);
        this.step = 0;
        cc.eventManager.addListener({
            event: cc.EventListener.TOUCH_ONE_BY_ONE,
            onTouchBegan: this.touched.bind(this)
        }, this);
        this.tutorialLabel = creator.createLabel("", 25);
        this.changeText(txt.tutorial.s0);
        this.addChild(this.tutorialLabel);
    }

    touched () {
        switch (this.step) {
            case 0:
                this.step++;
                this.changeText(txt.tutorial.s1);
                this.start(left);
                break;
            case 2:
                this.step++;
                this.changeText(txt.tutorial.s3);
                if (this.chessmen[3][1] === Chessman.common &&
                    this.chessmen[1][3] === Chessman.common) {
                    this.chessmen[3][1] = Chessman.addCol;
                    this.chessmen[1][3] = Chessman.delCol;
                }
                else {
                    this.chessmen[4][1] = Chessman.addCol;
                    this.chessmen[1][4] = Chessman.delCol;
                }
                this.updateChessboard();
                break;
            case 3:
                this.step++;
                this.changeText(txt.tutorial.s4);
                if (this.chessmen[3][3] === Chessman.common) {
                    this.chessmen[3][3] = Chessman.flip;
                }
                else {
                    this.chessmen[2][2] = Chessman.flip;
                }
                this.updateChessboard();
                break;
            case 4:
                this.step++;
                this.changeText(txt.tutorial.s5);
                break;
            case 5:
                this.step++;
                this.changeText(txt.tutorial.s6);
                break;
            case 6:
                cc.director.runScene(new MainScene());
                break;
        }
    }

    changeText (text) {
        this.tutorialLabel.string = text;
        this.tutorialLabel.setPosition(this.tutorialLabel.width / 2 + 20,
            size.height - this.tutorialLabel.height / 2 - 60);
    }

    onChangedTurn () {
        if (this.step === 1) {
            this.step++;
            this.changeText(txt.tutorial.s2);
            this.stopTimer();
        }
    }

    onWin (timeout) {
        if (this.turn === left && timeout) {
            this.addChild(new ResultLayer(txt.result.youOutOfTime, cc.color(0, 0, 0)));
        }
    }
}
