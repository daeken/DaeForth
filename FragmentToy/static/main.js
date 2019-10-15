$(() => {
	var cvs = $('#canvas')[0];
	cvs.width = 800 * 2;
	cvs.height = 600 * 2;
	var ctx = cvs.getContext('webgl');
	ctx.getExtension('OES_texture_float');
	ctx.getExtension('OES_standard_derivatives');
	
	var clean = str => str.replace(/[\uFEFF\0]/g, '').replace(/(^\n+|\n$)/g, '');
	
	var p, v, f, buf;
	function setupShader(code) {
		if(v !== undefined) {
			ctx.deleteShader(v);
			v = undefined;
		}
		if(f !== undefined) {
			ctx.deleteShader(f);
			f = undefined;
		}
		if(p !== undefined) {
			ctx.deleteProgram(p);
			p = undefined;
		}
		if(buf !== undefined) {
			ctx.deleteBuffer(buf);
			buf = undefined;
		}
		p = ctx.createProgram();
		v = ctx.createShader(ctx.VERTEX_SHADER);
		ctx.shaderSource(v, 'precision highp float; attribute vec4 p; varying vec2 position; void main() { position = p.xy * 2.0; gl_Position = vec4(p.xyz-1.0, 1); }');
		ctx.compileShader(v);
		if(!ctx.getShaderParameter(v, ctx.COMPILE_STATUS)) {
			console.log('Failed to compile vertex shader.');
			console.log(ctx.getShaderInfoLog(v));
			return;
		}
		ctx.attachShader(p, v);
		f = ctx.createShader(ctx.FRAGMENT_SHADER);
		ctx.shaderSource(f, code);
		ctx.compileShader(f);
		if(!ctx.getShaderParameter(f, ctx.COMPILE_STATUS)) {
			console.log('Failed to compile fragment shader.');
			logeditor.setValue(logeditor.getValue() + '\n\n/*\n' + clean(ctx.getShaderInfoLog(f)) + '\n*/');
			logeditor.clearSelection();
			return;
		}
		ctx.attachShader(p, f);
		ctx.linkProgram(p);
		if(!ctx.getProgramParameter(p, ctx.LINK_STATUS)) {
			console.log('Failed to link program.');
			logeditor.setValue(logeditor.getValue() + '\n\n/*\n' + clean(ctx.getProgramInfoLog(p)) + '\n*/');
			logeditor.clearSelection();
			return;
		}

		ctx.bindBuffer(ctx.ARRAY_BUFFER, buf = ctx.createBuffer());
		ctx.bufferData(ctx.ARRAY_BUFFER, new Float32Array([0,0,2,0,0,2,2,0,2,2,0,2]), ctx.STATIC_DRAW);
		ctx.vertexAttribPointer(0, 2, ctx.FLOAT, 0, 0, 0);
		
		render();
	}
	
	function render() {
		ctx.viewport(0, 0, 800 * 2, 600 * 2);
		ctx.enableVertexAttribArray(ctx.getAttribLocation(p, 'p'));
		ctx.useProgram(p);
		ctx.uniform2f(ctx.getUniformLocation(p, 'resolution'), 800 * 2, 600 * 2);

		ctx.drawArrays(4, 0, 6);
		ctx.finish();
	}
	
	function compile() {
		$.ajax({
			url: '/api/compile',
			method: 'POST',
			//contentType: "application/x-www-form-urlencoded",
			data: { code: editor.getValue() }
		}).done(x => {
			var comb = '#extension GL_OES_standard_derivatives : enable\nprecision highp float;\n';
			x.Errors = x.Errors != null ? clean(x.Errors) : '';
			x.Code = x.Code != null ? clean(x.Code) : null;
			if(x.Errors.replace(/[\n\r ]/g, '').length != 0)
				comb += '/*\n' + x.Errors + '\n*/\n\n';
			if(x.Code !== null)
				comb += x.Code;
			logeditor.setValue(comb);
			logeditor.clearSelection();
			setupShader(comb);
		})
	}

	var editor = ace.edit("editor");
	editor.setTheme("ace/theme/monokai");
	//editor.session.setMode("ace/mode/javascript");
	editor.setShowPrintMargin(false);
	editor.setShowFoldWidgets(false);
	var logeditor = ace.edit("log");
	logeditor.setTheme("ace/theme/monokai");
	logeditor.session.setMode("ace/mode/glsl");
	logeditor.setShowPrintMargin(false);
	logeditor.setShowFoldWidgets(false);
	logeditor.setReadOnly(true);
	var cursorChange = () => {
		var pos = editor.getCursorPosition();
		$('#position').text((pos.row + 1) + ":" + (pos.column + 1));
	};
	editor.selection.on('changeCursor', cursorChange);
	var curTimeout = null;
	var first = true;
	editor.on('change', () => {
		if(first) { first = false; return; }
		if(curTimeout !== null)
			clearTimeout(curTimeout);
		curTimeout = setTimeout(() => { curTimeout = null; compile() }, 250);
	});
	cursorChange();
	$.ajax({url: '/api/code'}).done(x => {
		x = x.Code;
		editor.setValue(x);
		editor.clearSelection();
		compile();
	});
});
