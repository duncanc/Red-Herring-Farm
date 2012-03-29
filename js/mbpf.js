
if (!window.mbpf) {
    window.mbpf = {};
}

var ROOT = -1;
var TOPLEFT = 0;
var TOPRIGHT = 1;
var BOTTOMLEFT = 2;
var BOTTOMRIGHT = 3;

/*

interface IMaskZoneData {
    int width;
    int height;
    
    // must return a zone number wherever 0 <= x < width, 0 <= y < height
    int zoneAt(int x, int y);
    
    optional String serialized;
};

interface IByteReader {
    unsigned byte readByte();
};

interface IByteWriter {
    void writeByte(unsigned byte b);
};

interface IMask {
    // (1 << size) is the width/height in pixels
    unsigned byte size;
    // true dimensions of the mask
    unsigned int width, height;
    IMaskNode rootNode;
};

interface IMaskNode {
    // either topLeft, topRight, bottomLeft, bottomRight are all defined,
    // or zone is.
    optional IMaskNode topLeft, topRight, bottomLeft, bottomRight;
    optional int zone;
    
    void load(MaskData maskData, int size, int x, int y);
    void serialize(byteWriter);
    void deserialize(byteReader);
};
    
*/

// implements: IMaskZoneData
mbpf.ImageMaskZoneData = function(imageData, maskColours) {
    this.width = imageData.width;
    this.height = imageData.height;
    var pixels = imageData.data;
    var pitch = imageData.width * 4;
    this.zoneAt = function(x, y) {
        var index = (y * pitch) + (x * 4);
        var r = pixels[index];
        var g = pixels[index+1];
        var b = pixels[index+2];
        var rgb = (r << 16) | (g << 8) | b;
        var nearest = 0, nearestDist;
        for (var i = 0, i_max = maskColours.length; i < i_max; i++) {
            var c = maskColours[i];
            if (c === rgb) {
                return i;
            }
            var r2 = (c >>> 16), g2 = (c >>> 8) & 0xFF, b2 = c & 0xFF;
            var dist = (r2-r)*(r2-r) + (g2-g)*(g2-g) + (b2-b)*(b2-b);
            if (i === 0 || dist < nearestDist) {
                nearest = i;
                nearestDist = dist;
            }
        }
        return nearest;
    }
}

// implements: IByteReader
mbpf.StringByteReader = function(str) {
    this.str = str;
    this.pos = 0;
}
mbpf.StringByteReader.prototype = {
    readByte: function() {
        return this.str.charCodeAt(this.pos++);
    }
}

// implements: IByteReader
mbpf.ArrayByteReader = function(arr) {
    this.arr = arr;
    this.pos = 0;
}
mbpf.StringByteReader.prototype = {
    readByte: function() {
        return this.arr[this.pos++];
    }
}

// implements: IByteWriter
mbpf.StringByteWriter = function() {
    this.buf = [];
}
mbpf.StringByteWriter.prototype = {
    writeByte: function(b) {
        this.buf.push(String.fromCharCode(b));
    },
    toString: function() {
        return this.buf.join('');
    }
}

// implements: IByteWriter
mbpf.ArrayByteWriter = function() {
    this.arr = [];
}
mbpf.ArrayByteWriter.prototype = {
    writeByte: function(b) {
        this.arr.push(b);
    },
    toArray: function() {
        return this.arr.slice();
    }
}

// implements: IMaskNode
mbpf.MaskNode = function(parent, position) {
    this.parentNode = parent;
    this.position = position;
}
mbpf.MaskNode.prototype = {
    parentNode: null,
    zone: 0,
    topLeft: null,
    topRight: null,
    bottomLeft: null,
    bottomRight: null,
    load: function(maskData, x, y, size) {
        this.x = x;
        this.y = y;
        this.size = size;
        if (x < 0 || y < 0 || x >= maskData.width || y >= maskData.height) {
            return;
        }
        if (size === 1) {
            this.zone = maskData.zoneAt(x, y);
            return;
        }
        var tl = new mbpf.MaskNode(this, TOPLEFT);
        var tr = new mbpf.MaskNode(this, TOPRIGHT);
        var bl = new mbpf.MaskNode(this, BOTTOMLEFT);
        var br = new mbpf.MaskNode(this, BOTTOMRIGHT);
        
        var halfSize = size >>> 1;
        tl.load(maskData, x, y, halfSize);
        tr.load(maskData, x + halfSize, y, halfSize);
        bl.load(maskData, x, y + halfSize, halfSize);
        br.load(maskData, x + halfSize, y + halfSize, halfSize);
        
        /*
        if (tr.x >= maskData.width) {
            tr.topLeft = tr.topRight = tr.bottomLeft = tr.bottomRight = null;
            br.topLeft = br.topRight = br.bottomLeft = br.bottomRight = null;
            tr.zone = tl.zone;
            br.zone = bl.zone;
            tr.zone = tl.zone;
            br.zone = bl.zone;
        }
        if (bl.y >= maskData.height) {
            bl.topLeft = bl.topRight = bl.bottomLeft = bl.bottomRight = null;
            br.topLeft = br.topRight = br.bottomLeft = br.bottomRight = null;
            bl.zone = tl.zone;
            br.zone = tr.zone;
        }
        */
        
        if (!tl.topLeft && !tr.topLeft && !bl.topLeft && !br.topLeft
                && tl.zone === tr.zone && tr.zone === bl.zone && bl.zone === br.zone) {
            this.zone = tl.zone;
        }
        else {
            this.topLeft = tl;
            this.topRight = tr;
            this.bottomLeft = bl;
            this.bottomRight = br;
        }
    },
    serialize: function(byteWriter) {
        if (this.topLeft) {
            byteWriter.writeByte(0xFF);
            this.topLeft.serialize(byteWriter);
            this.topRight.serialize(byteWriter);
            this.bottomLeft.serialize(byteWriter);
            this.bottomRight.serialize(byteWriter);
        }
        else {
            byteWriter.writeByte(this.zone);
        }
    },
    deserialize: function(byteReader, x, y, size) {
        var b = byteReader.readByte();
        this.x = x;
        this.y = y;
        this.size = size;
        if (b === 0xFF) {
            this.topLeft = new mbpf.MaskNode(this, TOPLEFT);
            this.topRight = new mbpf.MaskNode(this, TOPRIGHT);
            this.bottomLeft = new mbpf.MaskNode(this, BOTTOMLEFT);
            this.bottomRight = new mbpf.MaskNode(this, BOTTOMRIGHT);
            var halfSize = size >>> 1;
            this.topLeft.deserialize(byteReader, x, y, halfSize);
            this.topRight.deserialize(byteReader, x + halfSize, y, halfSize);
            this.bottomLeft.deserialize(byteReader, x, y + halfSize, halfSize);
            this.bottomRight.deserialize(byteReader, x + halfSize, y + halfSize, halfSize);
        }
        else {
            this.zone = b;
        }
    },
    getKey: function() {
        var key = [];
        for (var node = this; node.position !== ROOT; node = node.parentNode) {
            key.unshift(node.position);
        }
        return key.join('');
    },
    unbrokenLine: function(x1, y1, x2, y2, isZoneEnabled) {
        //if ((x1 < -1 && x2 <= -1) || (y1 < -1 && y2 <= -1) || (x1 >= 1 && x2 >= 1) || (y1 >= 1 && y2 >= 1)) return true;
        if (x1 <= x2) {
            if (x1 < -1 && x2 <= -1) return true;
        }
        else {
            if (x2 < -1 && x1 <= -1) return true;
        }
        if (y1 <= y2) {
            if (y1 < -1 && y2 <= -1) return true;
        }
        else {
            if (y2 < -1 && y1 <= -1) return true;
        }
        if ((x1 >= 1 && x2 >= 1) || (y1 >= 1 && y2 >= 1)) return true;
        var y_diff = y2 - y1;
        var x_diff = x1 - x2;
        var c = (x2 * y1) - (x1 * y2);
        var f_TL = -y_diff - x_diff + c; // x = -1, y = -1 (top left)
        var f_BR = y_diff + x_diff + c; // x = 1, y = 1 (bottom right)
        var f_BL = -y_diff + x_diff + c; // x = -1, y = 1 (bottom left)
        var f_TR = y_diff - x_diff + c; // x = 1, y = -1 (top right)
        // if through top left corner but NOT through top right or bottom left
        if (f_TL === 0 && ((f_TR > 0 && f_BL > 0) || (f_TR < 0 && f_BL < 0))) return true;
        // NOTE: top left is different from the others. "touching" any of the other corners
        // does not mean the line has gone through the square.
        if ((f_TL > 0 && f_TR >= 0 && f_BL >= 0 && f_BR >= 0) || (f_TL < 0 && f_TR <= 0 && f_BL <= 0 && f_BR <= 0)) return true;
        if (!this.topLeft) {
            /*
            if (!zoneEnabled[this.zone] && Math.abs(x2 - x1) === Math.abs(y2 - y1)) {
                ExternalInterface.call("console.log", "x(" + x1 + "," + y1 + " -> " + x2 + "," + y2 + ") [" + x_diff + ", " + y_diff + ", " + c + "; " + f_TL + ", " + f_TR + ", " + f_BL + ", " + f_BR + "]");
            }
            */
            return isZoneEnabled(this.zone);
        }
        else {
            return this.topLeft.unbrokenLine((x1 + 0.5) * 2, (y1 + 0.5) * 2, (x2 + 0.5) * 2, (y2 + 0.5) * 2, isZoneEnabled)
                && this.topRight.unbrokenLine((x1 - 0.5) * 2, (y1 + 0.5) * 2, (x2 - 0.5) * 2, (y2 + 0.5) * 2, isZoneEnabled)
                && this.bottomLeft.unbrokenLine((x1 + 0.5) * 2, (y1 - 0.5) * 2, (x2 + 0.5) * 2, (y2 - 0.5) * 2, isZoneEnabled)
                && this.bottomRight.unbrokenLine((x1 - 0.5) * 2, (y1 - 0.5) * 2, (x2 - 0.5) * 2, (y2 - 0.5) * 2, isZoneEnabled);
        }
    },
    addNeighbours: function(v) {
        var tempNode;
        var parentNode = this.parentNode;
        if (!parentNode) return;
        switch(this.position) {
            case TOPLEFT:
                parentNode.topRight.addInnerLeftNodes(v);
                parentNode.bottomLeft.addInnerTopNodes(v);
                v.push(parentNode.bottomRight.getInnerTopLeftRecursive());
                tempNode = this.getOuterTopNode();
                if (tempNode) tempNode.addInnerBottomNodes(v);
                tempNode = this.getOuterLeftNode();
                if (tempNode) tempNode.addInnerRightNodes(v);
                tempNode = this.getOuterTopLeftNode();
                if (tempNode && v.indexOf(tempNode) === -1) v.push(tempNode);
                tempNode = this.getOuterBottomLeftNode();
                if (tempNode && v.indexOf(tempNode) === -1) v.push(tempNode);
                tempNode = this.getOuterTopRightNode();
                if (tempNode && v.indexOf(tempNode) === -1) v.push(tempNode);
                break;
            case TOPRIGHT:
                parentNode.topLeft.addInnerRightNodes(v);
                parentNode.bottomRight.addInnerTopNodes(v);
                v.push(parentNode.bottomLeft.getInnerTopRightRecursive());
                tempNode = this.getOuterTopNode();
                if (tempNode) tempNode.addInnerBottomNodes(v);
                tempNode = this.getOuterRightNode();
                if (tempNode) tempNode.addInnerLeftNodes(v);
                tempNode = this.getOuterTopRightNode();
                if (tempNode && v.indexOf(tempNode) === -1) v.push(tempNode);
                tempNode = this.getOuterBottomRightNode();
                if (tempNode && v.indexOf(tempNode) === -1) v.push(tempNode);
                tempNode = this.getOuterTopLeftNode();
                if (tempNode && v.indexOf(tempNode) === -1) v.push(tempNode);
                break;
            case BOTTOMLEFT:
                parentNode.topLeft.addInnerBottomNodes(v);
                parentNode.bottomRight.addInnerLeftNodes(v);
                v.push(this.parentNode.topRight.getInnerBottomLeftRecursive());
                tempNode = this.getOuterBottomNode();
                if (tempNode) tempNode.addInnerTopNodes(v);
                tempNode = this.getOuterLeftNode();
                if (tempNode) tempNode.addInnerRightNodes(v);
                tempNode = this.getOuterBottomLeftNode();
                if (tempNode && v.indexOf(tempNode) === -1) v.push(tempNode);
                tempNode = this.getOuterBottomRightNode();
                if (tempNode && v.indexOf(tempNode) === -1) v.push(tempNode);
                tempNode = this.getOuterTopLeftNode();
                if (tempNode && v.indexOf(tempNode) === -1) v.push(tempNode);
                break;
            case BOTTOMRIGHT:
                parentNode.topRight.addInnerBottomNodes(v);
                parentNode.bottomLeft.addInnerRightNodes(v);
                v.push(parentNode.topLeft.getInnerBottomRightRecursive());
                tempNode = this.getOuterBottomNode();
                if (tempNode) tempNode.addInnerTopNodes(v);
                tempNode = this.getOuterRightNode();
                if (tempNode) tempNode.addInnerLeftNodes(v);
                tempNode = this.getOuterBottomRightNode();
                if (tempNode && v.indexOf(tempNode) === -1) v.push(tempNode);
                tempNode = this.getOuterBottomLeftNode();
                if (tempNode && v.indexOf(tempNode) === -1) v.push(tempNode);
                tempNode = this.getOuterTopRightNode();
                if (tempNode && v.indexOf(tempNode) === -1) v.push(tempNode);
                break;
        }
    },
    addInnerLeftNodes: function(v) {
        if (this.topLeft) {
            this.topLeft.addInnerLeftNodes(v);
            this.bottomLeft.addInnerLeftNodes(v);
        }
        else {
            v.push(this);
        }
    },
    addInnerRightNodes: function(v) {
        if (this.topLeft) {
            this.topRight.addInnerRightNodes(v);
            this.bottomRight.addInnerRightNodes(v);
        }
        else {
            v.push(this);
        }
    },
    addInnerTopNodes: function(v) {
        if (this.topLeft) {
            this.topLeft.addInnerTopNodes(v);
            this.topRight.addInnerTopNodes(v);
        }
        else {
            v.push(this);
        }
    },
    addInnerBottomNodes: function(v) {
        if (this.topLeft) {
            this.bottomLeft.addInnerBottomNodes(v);
            this.bottomRight.addInnerBottomNodes(v);
        }
        else {
            v.push(this);
        }
    },
    getInnerTopLeftRecursive: function() {
        return (!this.topLeft) ? this : this.topLeft.getInnerTopLeftRecursive();
    },
    getInnerTopRightRecursive: function() {
        return (!this.topLeft) ? this : this.topRight.getInnerTopRightRecursive();
    },
    getInnerBottomLeftRecursive: function() {
        return (!this.topLeft) ? this : this.bottomLeft.getInnerBottomLeftRecursive();
    },
    getInnerBottomRightRecursive: function() {
        return (!this.topLeft) ? this : this.bottomRight.getInnerBottomRightRecursive();
    },
    getInnerBottomLeftNonRecursive: function() {
        return this.bottomLeft || this;
    },
    getInnerBottomRightNonRecursive: function() {
        return this.bottomRight || this;
    },
    getInnerTopLeftNonRecursive: function() {
        return this.topLeft || this;
    },
    getInnerTopRightNonRecursive: function() {
        return this.topRight || this;
    },
    getOuterTopNode: function() {
        var tempNode;
        switch(this.position) {
            case TOPLEFT:
                tempNode = this.parentNode.getOuterTopNode();
                return tempNode && tempNode.getInnerBottomLeftNonRecursive();
            case TOPRIGHT:
                tempNode = this.parentNode.getOuterTopNode();
                return tempNode && tempNode.getInnerBottomRightNonRecursive();
            case BOTTOMRIGHT:
                return this.parentNode.topRight;
            case BOTTOMLEFT:
                return this.parentNode.topLeft;
        }
        return null;
    },
    getOuterBottomNode: function() {
        var tempNode;
        switch(this.position) {
            case TOPLEFT:
                return this.parentNode.bottomLeft;
            case TOPRIGHT:
                return this.parentNode.bottomRight;
            case BOTTOMRIGHT:
                tempNode = this.parentNode.getOuterBottomNode();
                return tempNode && tempNode.getInnerTopRightNonRecursive();
            case BOTTOMLEFT:
                tempNode = this.parentNode.getOuterBottomNode();
                return tempNode && tempNode.getInnerTopLeftNonRecursive();
        }
        return null;
    },
    getOuterLeftNode: function() {
        var tempNode;
        switch(this.position) {
            case TOPLEFT:
                tempNode = this.parentNode.getOuterLeftNode();
                return tempNode && tempNode.getInnerTopRightNonRecursive();
            case TOPRIGHT:
                return this.parentNode.topLeft;
            case BOTTOMRIGHT:
                return this.parentNode.bottomLeft;
            case BOTTOMLEFT:
                tempNode = this.parentNode.getOuterLeftNode();
                return tempNode && tempNode.getInnerBottomRightNonRecursive();
            default:
                return null;
        }
    },
    getOuterRightNode: function() {
        var tempNode;
        switch(this.position) {
            case TOPLEFT:
                return this.parentNode.topRight;
            case TOPRIGHT:
                tempNode = this.parentNode.getOuterRightNode();
                return tempNode && tempNode.getInnerTopLeftNonRecursive();
            case BOTTOMRIGHT:
                tempNode = this.parentNode.getOuterRightNode();
                return tempNode && tempNode.getInnerBottomLeftNonRecursive();
            case BOTTOMLEFT:
                return this.parentNode.bottomRight;
            default:
                return null;
        }
    },
    getOuterTopLeftNode: function() {
        var tempNode;
        switch(this.position) {
            case TOPLEFT:
                tempNode = this.parentNode.getOuterTopLeftNode();
                return tempNode && tempNode.getInnerBottomRightRecursive();
            case TOPRIGHT:
                tempNode = this.parentNode.getOuterTopNode();
                return tempNode && tempNode.getInnerBottomLeftNonRecursive().getInnerBottomRightRecursive();
            case BOTTOMRIGHT:
                return this.parentNode.topLeft;
            case BOTTOMLEFT:
                tempNode = this.parentNode.getOuterLeftNode();
                return tempNode && tempNode.getInnerTopRightNonRecursive().getInnerBottomRightRecursive();
            default:
                return null;
        }
    },
    getOuterTopRightNode: function() {
        var tempNode;
        switch(this.position) {
            case TOPLEFT:
                tempNode = this.parentNode.getOuterTopNode();
                return tempNode && tempNode.getInnerBottomRightNonRecursive().getInnerBottomLeftRecursive();
            case TOPRIGHT:
                tempNode = this.parentNode.getOuterTopRightNode();
                return tempNode && tempNode.getInnerBottomLeftRecursive();
            case BOTTOMRIGHT:
                tempNode = this.parentNode.getOuterRightNode();
                return tempNode && tempNode.getInnerTopLeftNonRecursive().getInnerBottomLeftRecursive();
            case BOTTOMLEFT:
                return this.parentNode.topRight;
            default:
                return null;
        }
    },
    getOuterBottomRightNode: function() {
        var tempNode;
        switch(this.position) {
            case TOPLEFT:
                return this.parentNode.bottomRight;
            case TOPRIGHT:
                tempNode = this.parentNode.getOuterRightNode();
                return tempNode && tempNode.getInnerBottomLeftNonRecursive().getInnerTopLeftRecursive();
            case BOTTOMRIGHT:
                tempNode = this.parentNode.getOuterBottomRightNode();
                return tempNode && tempNode.getInnerTopLeftRecursive();
            case BOTTOMLEFT:
                tempNode = this.parentNode.getOuterBottomNode();
                return tempNode && tempNode.getInnerTopRightNonRecursive().getInnerTopLeftRecursive();
            default:
                return null;
        }
    },
    getOuterBottomLeftNode: function() {
        var tempNode;
        switch(this.position) {
            case TOPLEFT:
                tempNode = this.parentNode.getOuterLeftNode();
                return tempNode && tempNode.getInnerBottomRightNonRecursive().getInnerTopRightRecursive();
            case TOPRIGHT:
                return this.parentNode.bottomLeft;
            case BOTTOMRIGHT:
                tempNode = this.parentNode.getOuterBottomNode();
                return tempNode && tempNode.getInnerTopLeftNonRecursive().getInnerTopRightRecursive();
            case BOTTOMLEFT:
                tempNode = this.parentNode.getOuterBottomLeftNode();
                return tempNode && tempNode.getInnerTopRightRecursive();
            default:
                return null;
        }
    },
    seedIsland: function(newIsland, touching) {
        var nodes = [this];
        var zone = this.zone;
        while (nodes.length > 0) {
            var node = nodes.pop();
            var otherIsland = node.island;
            if (otherIsland) {
                if (otherIsland !== newIsland) { otherIsland.addTouching(newIsland); newIsland.addTouching(otherIsland); }
                continue;
            }
            if (node.zone === zone) {
                node.island = newIsland;
                node.addNeighbours(nodes);
            }
            else {
                touching.push(node);
            }
        }
        return newIsland;
    }
}

// implements: IMask
mbpf.Mask = function() {
}
mbpf.Mask.prototype = {
    load: function(maskData) {
        var newSize = 8;
        while ((1 << newSize) < maskData.width || (1 << newSize) < maskData.height) {
            newSize++;
        }
        var newRoot = new mbpf.MaskNode(null, ROOT);
        newRoot.load(maskData, 0, 0, 1 << newSize);
        this.size = newSize;
        this.rootNode = newRoot;
        this.findIslands();
    },
    serialize: function(byteWriter) {
        if (byteWriter) {
            byteWriter.writeByte(this.size);
            this.rootNode.serialize(byteWriter);
        }
        else {
            byteWriter = new mbpf.StringByteWriter();
            byteWriter.writeByte(this.size);
            this.rootNode.serialize(byteWriter);
            return byteWriter.toString();
        }
    },
    // note: it is intended that you would actually
    // replace this method with your own function
    // when you get a Mask instance
    isZoneEnabled: function(zoneId) {
        return (zoneId !== 0);
    },
    deserialize: function(bytes) {
        var byteReader = (typeof bytes === "string") ? new mbpf.StringByteReader(bytes) : bytes;
        var newSize = byteReader.readByte();
        var newRoot = new mbpf.MaskNode(null, ROOT);
        newRoot.deserialize(byteReader, 0, 0, 1 << newSize);
        this.size = newSize;
        this.rootNode = newRoot;
        this.findIslands();
    },
    nodeAt: function(x, y) {
        var node = this.rootNode;
        var nodeSize = 1 << this.size;
        while (node.topLeft) {
            nodeSize = nodeSize >> 1;
            if (x < nodeSize) {
                if (y < nodeSize) {
                    node = node.topLeft;
                }
                else {
                    y -= nodeSize;
                    node = node.bottomLeft;
                }
            }
            else {
                x -= nodeSize;
                if (y < nodeSize) {
                    node = node.topRight;
                }
                else {
                    y -= nodeSize;
                    node = node.bottomRight;
                }
            }
        }
        return node;
    },
    unbrokenLine: function(x1, y1, x2, y2) {
        x1 += 0.5;
        y1 += 0.5;
        x2 += 0.5;
        y2 += 0.5;
        var rootSize = 1 << this.size;
        x1 = (x1*2 / rootSize) - 1;
        y1 = (y1*2 / rootSize) - 1;
        x2 = (x2*2 / rootSize) - 1;
        y2 = (y2*2 / rootSize) - 1;
        return this.rootNode.unbrokenLine(x1, y1, x2, y2, this.isZoneEnabled);
    },
    findPath: function(x1, y1, x2, y2) {
        if (this.unbrokenLine(x1, y1, x2, y2)) {
            return [{x:x2, y:y2}];
        }
        var firstNode = this.nodeAt(x1, y1);
        var lastNode = this.nodeAt(x2, y2);
        if (!firstNode.island.canReachIsland(lastNode.island)) {
            return [{x:x1, y:y1}];
        }
        var search = new mbpf.Search(this, x1, y1, x2, y2);
        search.go();
        var points = [];
        var closestFound = search.closestFound;
        if (closestFound.node === search.endNode) {
            points.push({x:x2, y:y2});
        }
        else {
            // nearest point on the final node
            points.push({
                x: Math.max(closestFound.node.x, Math.min(closestFound.node.x + closestFound.node.size, x2)),
                y: Math.max(closestFound.node.y, Math.min(closestFound.node.y + closestFound.node.size, y2))
            });
        }
        for (var nodeData = search.closestFound; nodeData; nodeData = nodeData.cameFrom) {
            points.push({x:nodeData.entryX, y:nodeData.entryY});
        }
        
        // path smoothing algorithm
        var point1_i = 0;
        while (point1_i < points.length-2) {
            var point1 = points[point1_i];
            var removeCount = (points.length - point1_i - 2) << 1;
            var smooth = false;
            while ((removeCount >>= 1) !== 0) {
                var point2 = points[point1_i + removeCount + 1];
                if (this.unbrokenLine(point1.x, point1.y, point2.x, point2.y)) {
                    smooth = true;
                    break;
                }
            }
            if (smooth) {
                points.splice(point1_i + 1, removeCount);
            }
            else {
                point1_i++;
            }
        }
        
        // reverse the points as we started with the last and worked our way back
        points.reverse();
        
        // remove the first node (as that is where we already are...)
        points.shift();
        
        return points;
    },
    findIslands: function() {
        var topLeft = this.rootNode;
        while (topLeft.topLeft) {
            topLeft = topLeft.topLeft;
        }
        var nodes = [topLeft];
        var islands = [];
        while (nodes.length > 0) {
            var node = nodes.pop();
            if (!node.island) {
                var newIsland = new mbpf.Island(this, islands.length, node.zone);
                node.seedIsland(newIsland, nodes);
                islands.push(newIsland);
            }
        }
        this.islands = islands;
        return islands;
    }
}

// returns true if node1Data should be investigated before node2Data
var nodeDataCompare = function(node1Data, node2Data) {
    var distFromStart1 = node1Data.distFromStart, distFromStart2 = node2Data.distFromStart;
    var distToEnd1 = node1Data.estimatedDistToEnd, distToEnd2 = node2Data.estimatedDistToEnd;
    if (node1Data.cameFrom && distToEnd1 < node1Data.cameFrom.estimatedDistToEnd) {
        if (node1Data.node.size > node2Data.node.size) {
            return true;
        }
    }
    return (distFromStart1+distToEnd1 < distFromStart2+distToEnd2);
}

mbpf.Search = function(mask, x1, y1, x2, y2) {
    this.nodeData = {};
    this.openSet = [];
    this.mask = mask;
    var startNodeData = this.getSearchData(mask.nodeAt(x1, y1));
    startNodeData.entryX = x1;
    startNodeData.entryY = y1;
    startNodeData.distFromStart = 0;
    startNodeData.estimatedDistToEnd = (x2 - x1)*(x2 - x1) + (y2 - y1)*(y2 - y1);
    this.addToOpenSet(startNodeData);
    this.endX = x2;
    this.endY = y2;
	this.endNode = mask.nodeAt(x2, y2);
    this.closestFound = startNodeData;
}
mbpf.Search.prototype = {
    nodeData: null,
    openSet: null,
    go: function() {
        var neighbours = [];
        var mask = this.mask;
        var openSet = this.openSet;
        var endX = this.endX, endY = this.endY;
        do {
            var nodeData = this.getNextFromOpenSet();
            var node = nodeData.node;
            if (node == this.endNode) {
                this.closestFound = nodeData;
                break;
            }
            nodeData.closed = true;
            neighbours.splice(0, neighbours.length);
            nodeData.node.addNeighbours(neighbours);
            for (var i = 0; i < neighbours.length; i++) {
                var neighbourData = this.getSearchData(neighbours[i]);
                var neighbour = neighbourData.node;
                if (neighbourData.closed || !mask.isZoneEnabled(neighbour.zone)) {
                    continue;
                }
                var newEntryX = Math.floor(
                    (Math.max(neighbour.x, node.x)
                    + Math.min(neighbour.x + neighbour.size, node.x + node.size)) / 2);
                var newEntryY = Math.floor(
                    (Math.max(neighbour.y, node.y)
                    + Math.min(neighbour.y + neighbour.size, node.y + node.size)) / 2);
                var xDiff = newEntryX - nodeData.entryX;
                var yDiff = newEntryY - nodeData.entryY;
                var tentativeDistance = neighbourData.distFromStart + (xDiff*xDiff + yDiff*yDiff);
                if (neighbourData.distFromStart === -1 || tentativeDistance < neighbourData.distFromStart) {
                    neighbourData.cameFrom = nodeData;
                    neighbourData.entryX = newEntryX;
                    neighbourData.entryY = newEntryY;
                    neighbourData.distFromStart = tentativeDistance;
                    neighbourData.estimatedDistToEnd = Math.pow(endX - newEntryX, 2) + Math.pow(endY - newEntryY, 2);
                    
                    if (neighbourData.estimateTotalDist() < this.closestFound.estimateTotalDist()) {
                        this.closestFound = neighbourData;
                    }
                }
                if (openSet.indexOf(neighbourData) == -1) this.addToOpenSet(neighbourData);
            }
        }
        while (openSet.length > 0);
        return true;
    },
    addToOpenSet: function(n) {
        var openSet = this.openSet;
        var new_i = openSet.length;
        openSet.push(n);
        while (new_i > 0) {
            var parent_i = (new_i - 1) >> 1;
            var parent = openSet[parent_i];
            if (nodeDataCompare(n, parent)) {
                openSet[parent_i] = n;
                openSet[new_i] = parent;
                new_i = parent_i;
            }
            else {
                break;
            }
        }
    },
    getSearchData: function(node) {
        var key = node.getKey();
        var data = this.nodeData[key];
        if (!data) {
            data = new mbpf.NodeSearchData(this.mask, node);
            this.nodeData[key] = data;
        }
        return data;
    },
    getNextFromOpenSet: function() {
        var openSet = this.openSet;
        var removed = openSet[0];
        
        // curious - if I do  openSet[0] = openSet.pop();   something seems to take forever?
        openSet[0] = openSet[openSet.length - 1];
        openSet.length = openSet.length - 1;
        
        var new_i = 0;
        while (true) {
            var left_i = (2 * new_i) + 1;
            var right_i = left_i + 1;
            var shortest_i = new_i;
            if (left_i < openSet.length
                    && (openSet[left_i].estimateTotalDist() < openSet[shortest_i].estimateTotalDist())) {
                shortest_i = left_i;
            }
            if (right_i < openSet.length
                    && (openSet[right_i].estimateTotalDist() < openSet[shortest_i].estimateTotalDist())) {
                shortest_i = right_i;
            }
            if (shortest_i !== new_i) {
                var temp = openSet[shortest_i];
                openSet[shortest_i] = openSet[new_i];
                openSet[new_i] = temp;
                new_i = shortest_i;
            }
            else {
                return removed;
            }
        }
    }
}

mbpf.NodeSearchData = function(mask, node) {
    this.node = node;
}
mbpf.NodeSearchData.prototype = {
    node: null,
    closed: false,
    entryX: null,
    entryY: null,
    cameFrom: null,
    distFromStart: -1,
    estimatedDistToEnd: null,
    estimateTotalDist: function() {
        return this.distFromStart + this.estimatedDistToEnd;
    }
}

mbpf.Island = function(mask, id, zone) {
    this.mask = mask;
    this.id = id;
    this.zone = zone;
    this.touching = [];
}
mbpf.Island.prototype = {
    addTouching: function(otherIsland) {
        if (otherIsland !== this && this.touching.indexOf(otherIsland) === -1) {
            this.touching.push(otherIsland);
        }
    },
	canReachIsland_aux: function(otherIsland, mask, checked) {
        if (!mask.isZoneEnabled(this.zone)) {
            return false;
        }
        if (this === otherIsland) {
            return true;
        }
		var touching = this.touching;
		for (var i = 0, i_max = touching.length; i < i_max; i++) {
			var checkIsland = touching[i];
			if (typeof checked[checkIsland.id] !== 'undefined') {
				continue;
			}
			checked[checkIsland.id] = true;
			if (checkIsland.canReachIsland_aux(otherIsland, mask, checked)) {
				return true;
			}
		}
		return false;
	},
	canReachIsland: function(otherIsland) {
		var mask = this.mask;
		if (!mask.isZoneEnabled(otherIsland.zone)) {
			return false;
		}
		var checked = {};
		checked[this.id] = true;
		return this.canReachIsland_aux(otherIsland, mask, checked);
    }
}
