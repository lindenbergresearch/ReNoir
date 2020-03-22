using Godot;
using static Util;

/// <summary>
///     Tool node to edit and preview a bitmap font in realtime.
/// </summary>
[Tool]
public class BitmapFont2D : Godot.Node2D {
	private Color _foreground;
	private Vector2 _glyphDimension;
	private Texture _imageTexture;
	private float _lineMargin;
	private int _offset;
	private Vector2 _scale;
	private string _text;


	/// <summary>
	///     Setup standard values
	/// </summary>
	public BitmapFont2D() {
		_glyphDimension = Vec(8, 16);
		_foreground = Color(1, 1, 1);
		_text = "The quick brown fox is dead. !\"§$\"%&/()=?><|#+*'´`/\\";
		_scale = Vec(1, 1);

		_offset = 32;

		BitmapFont = new BitmapFont();
	}


	private BitmapFont BitmapFont { get; }


	/// <summary>
	/// </summary>
	[Export]
	public Texture ImageTexture {
		get => _imageTexture;
		set {
			_imageTexture = value;
			ConfigureBitmapFont();
			Refresh();
		}
	}

	/// <summary>
	/// </summary>
	[Export]
	public Vector2 GlyphDimension {
		get => _glyphDimension;
		set {
			var v = value;

			if ((int) v.x == 0) v.x = 1;
			if ((int) v.y == 0) v.y = 1;

			_glyphDimension = v;
			ConfigureBitmapFont();
			Refresh();
		}
	}

	/// <summary>
	/// </summary>
	[Export]
	public int Offset {
		get => _offset;
		set {
			_offset = value;
			ConfigureBitmapFont();
			Refresh();
		}
	}

	/// <summary>
	/// </summary>
	[Export]
	public Color Foreground {
		get => _foreground;
		set {
			_foreground = value;
			Refresh();
		}
	}

	/// <summary>
	/// </summary>
	[Export(PropertyHint.MultilineText)]
	public string Text {
		get => _text;
		set {
			_text = value;
			Refresh();
		}
	}

	/// <summary>
	/// </summary>
	[Export]
	public Vector2 TextScale {
		get => _scale;
		set {
			_scale = value;
			Refresh();
		}
	}

	[Export]
	public Vector2 CharsDimension { get; set; }


	[Export]
	public float LineMargin {
		get => _lineMargin;
		set {
			_lineMargin = value;
			Refresh();
		}
	}


	private void ConfigureBitmapFont() {
		BitmapFont.imageTexture = _imageTexture;
		BitmapFont.Offset = _offset;
		BitmapFont.CharsDimension = CharsDimension;
		BitmapFont.GlyphDimension = _glyphDimension;
	}


	/// <summary>
	///     Simulates a put pixel taking scale into account.
	/// </summary>
	/// <param name="pos">The position (in normal coordinates)</param>
	/// <param name="color">The color</param>
	private void PutPixel(Vector2 pos, Color color) {
		DrawRect(new Rect2(pos * _scale, _scale), color);
	}


	/// <summary>
	/// </summary>
	/// <param name="pos"></param>
	/// <param name="text"></param>
	private void DrawText(Vector2 pos, string text) {
		for (var c = 0; c < text.Length; c++) {
			var index = Mathf.Clamp(text[c] - _offset, 0, BitmapFont.Glyphs.Length - 1);
			var glyph = BitmapFont.Glyphs[index];


			for (var y = 0; y < GlyphDimension.y; y++)
			for (var x = 0; x < GlyphDimension.x; x++)
				if (glyph.PixelAt(x, y))
					PutPixel(Vec(x + c * GlyphDimension.x, y) + pos, Foreground);
		}
	}


	/// <summary>
	///     Draw font
	/// </summary>
	public override void _Draw() {
		if (_imageTexture == null || _imageTexture.GetData() == null || BitmapFont == null || BitmapFont.Glyphs.Length == 0) return;

		var lines = Text.Split('\n');

		var yoffset = 0;

		foreach (var line in lines) DrawText(Vec(0, yoffset++ * (GlyphDimension.y + _lineMargin)), line);
	}


	private void Refresh() {
		if (_imageTexture == null || _imageTexture.GetData() == null) return;

		BitmapFont.process();

		CharsDimension = BitmapFont.CharsDimension;

		Update();
	}


	/// <summary>
	/// </summary>
	public override void _Ready() {
	}
}