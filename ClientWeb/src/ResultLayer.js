class ResultLayer extends cc.Layer {
    constructor(text, textcolor) {
        super();
        const background = new cc.LayerColor(cc.color(255, 255, 255, 150));
        this.addChild(background);
        const label = creator.createLabel(text, 50, textcolor);
        label.textAlign = cc.TEXT_ALIGNMENT_CENTER;
        label.boundingWidth = size.width;
        label.setPosition(this.width / 2, this.height / 2);
        this.addChild(label);
    }
}
