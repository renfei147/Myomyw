class AboutScene extends MenuScene {
    constructor() {
        super(txt.options.about, function () {
            cc.director.runScene(new OptionScene());
        });

        const text = "Myomyw Beta 0.7\nCopyright © 2016 Infinideas";
        const label = creator.createLabel(text, 20);
        label.textAlign = cc.TEXT_ALIGNMENT_CENTER;
        label.setPosition(size.width / 2, size.height / 2);
        this.addChild(label);
    }
}
