class OnlineGameScene extends GameScene {
    constructor() {
        super(player.name, txt.names.opponent, left, null, true);
        this.opponentName = null;
        this.disconnected = false;
        this.movingCol = null;
        this.firstMove = true;
        this.serverReason = null; //null为未定胜负
        this.clientReason = null;
        this.roomLabel = creator.createLabel(txt.online.connecting, 25);
        this.roomLabel.setPosition(size.width - this.roomLabel.width / 2 - 10, this.roomLabel.height / 2 + 10);
        this.addChild(this.roomLabel);

        let io = window.SocketIO || window.io;
        this.socket = io.connect(player.server, {"force new connection": true});
        this.socket.on("connect", this.onConnect.bind(this));
        this.socket.on("error", this.onError.bind(this));
        this.socket.on("start", this.onStart.bind(this));
        this.socket.on("nextChessman", this.onNextChessman.bind(this));
        this.socket.on("move", this.onMove.bind(this));
        this.socket.on("endTurn", this.onEndTurn.bind(this));
        this.socket.on("endGame", this.onEndGame.bind(this));
        this.socket.on("disconnect", this.onDisconnect.bind(this));
    }

    onExit () {
        this._super();
        if (!this.disconnected) {
            this.socket.disconnect();
            this.disconnected = true;
        }
    }

    win () {
        let str = "";
        switch (this.clientReason) {
            case EndReason.opponentLeft:
                str = txt.result.opponentLeft;
                break;
            case EndReason.youWin:
                str = txt.result.youWin;
                break;
            case EndReason.opponentWins:
                str = format(txt.result.win, this.opponentName);
                break;
            case EndReason.youOutOfTime:
                str = txt.result.youOutOfTime;
                str += "\n";
                str += format(txt.result.win, this.opponentName);
                break;
            case EndReason.opponentOutOfTime:
                str = format(txt.result.outOfTime, this.opponentName);
                str += "\n";
                str += txt.result.youWin;
                break;
        }
        this.addChild(new ResultLayer(str, cc.color(0, 0, 0)));
    }

    //GameScene的回调
    onBeganMoving (col, last) {
        if (this.turn === left && !this.disconnected) {
            if (this.firstMove) {
                this.socket.emit("move", JSON.stringify({col: col}));
                this.firstMove = false;
            } else {
                this.socket.emit("move", "");
            }
        }
    }

    onChangedTurn () {
        if (this.turn === right) {
            if (!this.disconnected) {
                this.socket.emit("endTurn", "");
            }
            this.movingCol = null;
        }
        else {
            this.firstMove = true;
        }
    }

    onWin (timeout) {
        if (timeout)
            this.clientReason = this.turn === left ? EndReason.youOutOfTime : EndReason.opponentOutOfTime;
        else
            this.clientReason = this.turn === left ? EndReason.opponentWins : EndReason.youWin;

        if (this.serverReason != null) {
            if (this.serverReason === this.clientReason)
                this.win();
            else {
                this.addChild(new ResultLayer(txt.result.differentResult, cc.color(0, 0, 0)));
                cc.log("server reason:" + this.serverReason);
                cc.log("client reason:" + this.clientReason);
            }
        }
    }

    //socket.io的回调
    onConnect () {
        this.socket.emit("match", JSON.stringify({name: player.name}));
        this.roomLabel.string = txt.online.waiting;
        this.roomLabel.setPosition(size.width - this.roomLabel.width / 2 - 10, this.roomLabel.height / 2 + 10);
    }

    onError (data) {
        cc.log(data);
    }

    onStart (data) {
        data = parseJson(data);
        this.roomLabel.string = format(txt.online.room, data.room);
        this.roomLabel.setPosition(size.width - this.roomLabel.width / 2 - 10, this.roomLabel.height / 2 + 10);
        this.opponentName = data.opponentName;
        this.rightNameLabel.string = data.opponentName;
        this.rightNameLabel.setPosition(size.width - this.rightNameLabel.width / 2 - 20, size.height - this.rightNameLabel.height / 2 - 20);
        this.start(data.side);
    }

    onNextChessman (data) {
        data = parseJson(data);
        this.setNextChessman(data.chessman);
    }

    onMove (data) {
        data = parseJson(data);
        if (this.turn === right) {
            if ("col" in data && this.movingCol === null) {
                this.movingCol = data.col;
            }
            if (this.action === Action.moving) {
                this.endMovingAtOnce();
            }
            this.move(this.movingCol);
        }
    }

    onEndTurn () {
        if (this.turn === right) {
            if (this.action === Action.moving) {
                this.endMovingAtOnce();
            }
            //推出翻转球后上面的endMovingAtOnce会切换回合，所以要再判断一次
            if (this.turn === right) {
                this.changeTurn();
            }
        }
    }

    onEndGame (data) {
        data = parseJson(data);
        this.playing = false;
        this.serverReason = data.reason;
        if (this.serverReason === EndReason.opponentLeft) {
            this.clientReason = this.serverReason;
            this.stopTimer();
            this.win();
        }
        else if (this.serverReason === EndReason.serverFull) {
            this.addChild(new ResultLayer(txt.online.serverFull, cc.color(0, 0, 0)));
        }
        else {
            if (this.clientReason != null) {
                if (this.clientReason === this.serverReason)
                    this.win();
                else {
                    this.addChild(new ResultLayer(txt.result.differentResult, cc.color(0, 0, 0)));
                    cc.log("server reason:" + this.serverReason);
                    cc.log("client reason:" + this.clientReason);
                }
            }
        }
    }

    onDisconnect () {
        this.disconnected = true;
        if (this.serverReason == null) {
            this.socket.disconnect();
            this.playing = false;
            this.addChild(new ResultLayer(txt.result.unknownDisconnection, cc.color(0, 0, 0)));
        }
    }
}

function parseJson(str) {
    if (cc.sys.isNative)
        return JSON.parse(str);
    else
        return str;
}
