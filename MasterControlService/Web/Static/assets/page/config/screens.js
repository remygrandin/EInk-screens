//== Class definition
var ScreenConfig = function () {
    function Screen(id, x, y, height, width, rotation, margin) {
        this.Container_constructor();
        this.Id = id;
        this.RealX = x;
        this.RealY = y;

        this.RealHeight = height;
        this.RealWidth = width;
        this.RealRotation = rotation;
        this.RealMargin = margin;


        this.setup();
    }
    var p = createjs.extend(Screen, createjs.Container);

    p.setup = function () {
        /*
        var text = new createjs.Text(this.Id, "20px Arial", "#000");
        text.textBaseline = "top";
        text.textAlign = "left";



        text.x = width / 2;
        text.y = 10;
        */

        var background = new createjs.Shape();
        background.graphics.beginStroke("black").drawRect(0, 0, this.RealWidth, this.RealHeight);


        this.addChild(background);

        /*
        this.on("click", this.handleClick);
        this.on("rollover", this.handleRollOver);
        this.on("rollout", this.handleRollOver);
        */
        this.cursor = "grab";

        //this.mouseChildren = false;

        //this.offset = Math.random() * 10;
        //this.count = 0;
    };

    let baseScreen = createjs.promote(Screen, "Container");



    var initCanvas = function() {
        let width = Math.round($("#screenPlacement").width());
        $("#screenPlacement").width("");

        $("#screenPlacement").attr("width", width);

        let stage = new createjs.Stage("screenPlacement");
        stage.enableMouseOver();

        var scr1 = stage.addChild(new baseScreen("id1", 100, 100, 800,600,0,0));
        scr1.y = 20;

        createjs.Ticker.on("tick", stage);
    }
  

    return {
        //== Init demos
        init: function() {
            initCanvas();
        }
    };
}();

//== Class initialization on page load
jQuery(document).ready(function() {
    ScreenConfig.init();
});