class WelcomeScene extends cc.Scene {
    constructor() {
        super();
        const background = new cc.Sprite(res.MainSceneBG_png);
        background.attr({
            scale: Math.max(size.width / background.width, size.height / background.width),
            anchorX: 0.5,
            anchorY: 1,
            x: size.width / 2,
            y: size.height,
            opacity: 100
        });
        this.addChild(background);

        const label = creator.createLabel(txt.welcomeScene.title, 40);
        label.textAlign = cc.TEXT_ALIGNMENT_CENTER;
        label.boundingWidth = size.width;
        label.setPosition(size.width / 2, size.height / 2 + 100);
        this.addChild(label);

        const okButton = creator.createButton(txt.welcomeScene.ok, cc.size(150, 50), function () {
            storage.setItem("playedBefore", "true");
            cc.director.runScene(new TutorialGameScene());
        });
        okButton.setPosition(size.width / 2 - 120, size.height / 2 - 50);
        this.addChild(okButton);

        const skipButton = creator.createButton(txt.welcomeScene.skip, cc.size(150, 50), function () {
            storage.setItem("playedBefore", "true");
            cc.director.runScene(new MainScene());
        });
        skipButton.setPosition(size.width / 2 + 120, size.height / 2 - 50);
        this.addChild(skipButton);
    }
}