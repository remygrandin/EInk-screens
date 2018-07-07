//== Class definition
var ScreenConfig = function () {
    function Screen(id, x, y, width, height, rotation, margin) {
        this.Container_constructor();
        this.Id = id;
        this.RealX = x;
        this.RealY = y;

        this.RealHeight = height;
        this.RealWidth = width;
        this.RealRotation = rotation;
        this.RealMargin = margin;

        this.on("click", this.handleClick);
        this.on("rollover", this.handleRollOver);
        this.on("rollout", this.handleRollOut);
        this.on("mousedown", this.handleMousedown);
        this.on("pressmove", this.handlePressmove);
        this.on("pressup", this.handlePressup);


        this.cursor = "pointer";

        this.mouseChildren = false;

        this.setup();
    }
    var p = createjs.extend(Screen, createjs.Container);

    var selectedScreen = null;

    p.setup = function () {
        this.x = this.RealX;
        this.y = this.RealY;

        this.removeChild(this.background);
        this.background = new createjs.Shape();
        this.background.graphics.append(createjs.Graphics.beginCmd);
        this.addChild(this.background);

        this.removeChild(this.backgroundSelected);
        this.backgroundSelected = new createjs.Shape();
        this.backgroundSelected.graphics.append(createjs.Graphics.beginCmd);
        this.addChild(this.backgroundSelected);

        this.removeChild(this.backgroundHover);
        this.backgroundHover = new createjs.Shape();
        this.backgroundHover.graphics.append(createjs.Graphics.beginCmd);
        this.addChild(this.backgroundHover);

        this.removeChild(this.marginBorder);
        this.marginBorder = new createjs.Shape();
        this.marginBorder.mouseEnabled = false;
        this.marginBorder.graphics.append(createjs.Graphics.beginCmd);
        this.addChild(this.marginBorder);

        this.removeChild(this.idText);
        this.idText = new createjs.Text();
        this.idText.set({
            text: this.Id,
            color: "#000000",
            font: "100px Arial",
            textBaseline: "middle",
            textAlign: "center"
        });
        this.addChild(this.idText);

        this.removeChild(this.zeroReticule);
        this.zeroReticule = new createjs.Shape();
        this.zeroReticule.graphics.setStrokeStyle(10);
        this.zeroReticule.graphics.beginStroke("#000");
        this.zeroReticule.graphics.moveTo(0, -20);
        this.zeroReticule.graphics.lineTo(0, 20);
        this.zeroReticule.graphics.moveTo(-20, 0);
        this.zeroReticule.graphics.lineTo(20, 0);
        this.zeroReticule.graphics.endStroke();
        this.addChild(this.zeroReticule);

        let roundness = 5;
        let area = null;
        let areaMargin = null;

        switch (this.RealRotation) {
            case 0:
                area = new createjs.Graphics.RoundRect(0, 0, this.RealWidth, this.RealHeight, roundness, roundness, roundness, roundness);
                this.idText.set({ x: this.RealWidth / 2, y: this.RealHeight / 2 });
                break;
            case 90:
                area = new createjs.Graphics.RoundRect(this.RealHeight * -1, 0, this.RealHeight, this.RealWidth, roundness, roundness, roundness, roundness);
                this.idText.set({ x: this.RealHeight / 2 * -1, y: this.RealWidth / 2 });
                break;
            case 180:
                area = new createjs.Graphics.RoundRect(this.RealWidth * -1, this.RealHeight * -1, this.RealWidth, this.RealHeight, roundness, roundness, roundness, roundness);
                this.idText.set({ x: this.RealWidth / 2 * -1, y: this.RealHeight / 2 * -1 });
                break;
            case 270:
                area = new createjs.Graphics.RoundRect(this.RealHeight * -1, this.RealWidth * -1, this.RealHeight, this.RealWidth, roundness, roundness, roundness, roundness);
                this.idText.set({ x: this.RealHeight / 2, y: this.RealWidth / 2 * -1 });
                break;

        }

        areaMargin = new createjs.Graphics.RoundRect(area.x - this.RealMargin, area.y - this.RealMargin, area.w + this.RealMargin * 2, area.h + this.RealMargin * 2, roundness, roundness, roundness, roundness);

        
        this.marginBorder.graphics.append(areaMargin);

        this.marginBorder.graphics.append(new createjs.Graphics.StrokeDash([40, 20]));
        this.marginBorder.graphics.append(new createjs.Graphics.Stroke("Gray"));

        this.background.graphics.append(area);
        this.backgroundHover.graphics.append(area);
        this.backgroundSelected.graphics.append(area);

        
        this.background.graphics.append(new createjs.Graphics.Fill("rgba(128,128,128,0.5)"));
        this.background.graphics.append(new createjs.Graphics.Stroke("Gray"));
        
        this.backgroundHover.graphics.append(new createjs.Graphics.Fill("LightBlue"));
        this.backgroundHover.graphics.append(new createjs.Graphics.Stroke("Gray"));

        this.backgroundSelected.graphics.append(new createjs.Graphics.Fill("LightGreen"));
        this.backgroundSelected.graphics.append(new createjs.Graphics.Stroke("Gray"));


        this.backgroundHover.visible = false;

        let isSelected = this == selectedScreen;

        this.idText.visible = this.showId || isSelected;
        this.marginBorder.visible = this.showBorder || isSelected;
        this.backgroundSelected.visible = isSelected;

    };

    p.showId = true;
    p.showBorder = true;

    p.handleRollOver = function () {
        this.backgroundHover.visible = true;

        this.idText.visible = true;
        this.marginBorder.visible = true;
    }

    p.handleRollOut = function () {
        this.backgroundHover.visible = false;

        let isSelected = this == selectedScreen;

        this.idText.visible = this.showId || isSelected;
        this.marginBorder.visible = this.showBorder || isSelected;
    }

    p.handleClick = function () {
        selectedScreen = this;
        $.each(this.parent.children, function(index, value) {
            value.backgroundSelected.visible = false;
        });
        this.backgroundSelected.visible = true;
    }

    p.moveOffsetX = null;
    p.moveOffsetY = null;

    p.handleMousedown = function (evt) {
        this.moveOffsetX = evt.localX;
        this.moveOffsetY = evt.localY;

        this.parent.sortChildren(function (obj1, obj2) {
            return (obj1.moveOffsetX != null || obj1.moveOffsetY != null);
        });
    }

    p.handlePressup = function (evt) {
        this.moveOffsetX = null;
        this.moveOffsetY = null;
    }

    p.handlePressmove = function (evt) {
        let parentCoord = this.localToLocal(evt.localX, evt.localY, this.parent);

        this.x = this.RealX = Math.round(parentCoord.x - this.moveOffsetX);
        this.y = this.RealY = Math.round(parentCoord.y - this.moveOffsetY);

        updateContainerScale();
    };

    let baseScreen = createjs.promote(Screen, "Container");

    var stage;
    var container;
    var containerBorder;
    var containerMargin = 10;

    var initCanvas = function () {
        $("#screenPlacement").attr("width", "");
        $("#screenPlacement").width("100%");

        let width = Math.round($("#screenPlacement").width());
        let height = Math.round($("#screenPlacement").attr("height"));

        $("#screenPlacement").attr("width", width);

        stage = new createjs.Stage("screenPlacement");
        stage.enableMouseOver(50);

        container = new createjs.Container();
        container.x = containerMargin;
        container.y = containerMargin;
        stage.addChild(container);






        let scr1 = container.addChild(new baseScreen("id1", 0, 0, 800, 600, 0, 100));

        let scr2 = container.addChild(new baseScreen("id2", 4000, 1000, 800, 600, 180, 100));

        let scr3 = container.addChild(new baseScreen("id3", 2000, 500, 800, 600, 90, 100));

        updateContainerScale();

        createjs.Ticker.on("tick", stage);

        new ResizeObserver(entries => {
            
            $("#screenPlacement").attr("width", "");
            $("#screenPlacement").width("100%");

            let width = Math.round($("#screenPlacement").width());
            let height = Math.round($("#screenPlacement").attr("height"));

            $("#screenPlacement").attr("width", width);

            updateContainerScale();
        }).observe($("#screenPlacement")[0]);

    }

    var updateContainerScale = function () {
        let TLX = 0;
        let TLY = 0;
        let BRX = 0;
        let BRY = 0;

        $.each(container.children, function (index, value) {

            // Calculating standard rotation
            let scrTLX = 0;
            let scrTLY = 0;

            let scrTRX = 0;
            let scrTRY = 0;

            let scrBRX = 0;
            let scrBRY = 0;

            let scrBLX = 0;
            let scrBLY = 0;

            switch (value.RealRotation) {
                case 0:
                    scrTLX = value.RealX;
                    scrTLY = value.RealY;

                    scrTRX = value.RealX + value.RealWidth;
                    scrTRY = value.RealY;

                    scrBRX = value.RealX + value.RealWidth;
                    scrBRY = value.RealY + value.RealHeight;

                    scrBLX = value.RealX;
                    scrBLY = value.RealY + value.RealHeight;
                    break;

                case 90:
                    scrTLX = value.RealX - value.RealHeight;
                    scrTLY = value.RealY;

                    scrTRX = value.RealX;
                    scrTRY = value.RealY;

                    scrBRX = value.RealX;
                    scrBRY = value.RealY + value.RealWidth;

                    scrBLX = value.RealX - value.RealHeight;
                    scrBLY = value.RealY + value.RealWidth;
                    break;
                case 180:
                    scrTLX = value.RealX - value.RealWidth;
                    scrTLY = value.RealY - value.RealHeight;

                    scrTRX = value.RealX;
                    scrTRY = value.RealY - value.RealHeight;

                    scrBRX = value.RealX;
                    scrBRY = value.RealY;

                    scrBLX = value.RealX - value.RealWidth;
                    scrBLY = value.RealY;
                    break;
                case 270:
                    scrTLX = value.RealX;
                    scrTLY = value.RealY - value.RealWidth;

                    scrTRX = value.RealX + value.RealHeight;
                    scrTRY = value.RealY - value.RealWidth;

                    scrBRX = value.RealX + value.RealHeight;
                    scrBRY = value.RealY;

                    scrBLX = value.RealX;
                    scrBLY = value.RealY;
                    break;

            }

            /*
            TLX = Math.min(scrTLX, TLX);
            TLY = Math.min(scrTLY, TLY);
            */
            BRX = Math.max(scrBRX, BRX);
            BRY = Math.max(scrBRY, BRY);

        });

        let containerWidth = parseFloat(Math.round($("#screenPlacement").attr("width"))) - containerMargin * 2;
        let containerHeight = parseFloat(Math.round($("#screenPlacement").attr("height"))) - containerMargin * 2;

        let scaleX = containerWidth / BRX;
        let scaleY = containerHeight / BRY;


        container.scale = Math.min(scaleX, scaleY);

        let width = Math.round($("#screenPlacement").attr("width"));
        let height = Math.round($("#screenPlacement").attr("height"));

        stage.removeChild(containerBorder);
        containerBorder = new createjs.Shape();
        containerBorder.graphics.beginStroke("grey").drawRoundRect(containerMargin, containerMargin, width - containerMargin * 2, height - containerMargin * 2, 5);
        stage.addChild(containerBorder);

        container.mask = containerBorder;
    }

    var initToggle = function() {
        $("#inpShowID").change(function () {
            let isChecked = $("#inpShowID").is(":checked");
            $.each(container.children, function(index, value) {
                value.showId = isChecked;
                value.setup();
            });
        });

        $("#inpShowMargin").change(function () {
            let isChecked = $("#inpShowMargin").is(":checked");
            $.each(container.children, function (index, value) {
                value.showBorder = isChecked;
                value.setup();
            });
        });
    }

    return {
        //== Init demos
        init: function () {
            initCanvas();
            initToggle();
        }
    };
}();

//== Class initialization on page load
jQuery(document).ready(function () {
    ScreenConfig.init();
});