﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Extensions;

namespace Hearthstone_Deck_Tracker.Utility.Themes
{
	public abstract class CardBarImageBuilder
	{
		protected const string BarImageDir = "Images/Bars";

		protected Card _card;
		protected string _themeDir;
		protected DrawingGroup _drawingGroup = new DrawingGroup();
		protected bool _hasAllRequired = false;
		protected bool _hasAllOptional = false;

		private readonly FontFamily _font = new FontFamily(
			new Uri("pack://application:,,,/"), "./resources/#Belwe Bd BT");

		private readonly Typeface _countType;

		private static readonly Rect _frameRect = new Rect(0, 0, 217, 34);
		private static readonly Rect _gemRect = new Rect(0, 0, 34, 34);
		private static readonly Rect _boxRect = new Rect(192, 0, 25, 34);
		private static readonly Rect _imageRect = new Rect(83, 0, 134, 34);

		protected static readonly Dictionary<ThemeElement, ThemeElementInfo> _required =
			new Dictionary<ThemeElement, ThemeElementInfo>
		{
			{ ThemeElement.DefaultFrame, new ThemeElementInfo("frame.png", _frameRect) },
			{ ThemeElement.DefaultGem, new ThemeElementInfo("gem.png", _gemRect) },
			{ ThemeElement.DefaultCountBox, new ThemeElementInfo("countbox.png", _boxRect) },
			{ ThemeElement.DarkOverlay, new ThemeElementInfo("dark.png", _frameRect) },
			{ ThemeElement.FadeOverlay, new ThemeElementInfo("fade.png", _frameRect) },
			{ ThemeElement.GoldenFrame, new ThemeElementInfo("frame_golden.png", _frameRect) },
			{ ThemeElement.CreatedIcon, new ThemeElementInfo("icon_created.png", _boxRect) },
			{ ThemeElement.LegendaryIcon, new ThemeElementInfo("icon_legendary.png", _boxRect) }
		};

		protected static readonly Dictionary<ThemeElement, ThemeElementInfo> _optional =
			new Dictionary<ThemeElement, ThemeElementInfo>
		{
			{ ThemeElement.CommonFrame, new ThemeElementInfo("frame_common.png", _frameRect) },
			{ ThemeElement.RareFrame, new ThemeElementInfo("frame_rare.png", _frameRect) },
			{ ThemeElement.EpicFrame, new ThemeElementInfo("frame_epic.png", _frameRect) },
			{ ThemeElement.LegendaryFrame, new ThemeElementInfo("frame_legendary.png", _frameRect) },
			{ ThemeElement.CommonGem, new ThemeElementInfo("gem_common.png", _gemRect) },
			{ ThemeElement.RareGem, new ThemeElementInfo("gem_rare.png", _gemRect) },
			{ ThemeElement.EpicGem, new ThemeElementInfo("gem_epic.png", _gemRect) },
			{ ThemeElement.LegendaryGem, new ThemeElementInfo("gem_legendary.png", _gemRect) },
			{ ThemeElement.CommonCountBox, new ThemeElementInfo("countbox_common.png", _boxRect) },
			{ ThemeElement.RareCountBox, new ThemeElementInfo("countbox_rare.png", _boxRect) },
			{ ThemeElement.EpicCountBox, new ThemeElementInfo("countbox_epic.png", _boxRect) },
			{ ThemeElement.LegendaryCountBox, new ThemeElementInfo("countbox_legendary.png", _boxRect) }
		};

		protected CardBarImageBuilder(Card card, string dir)
		{
			_card = card;
			_themeDir = dir;
			_countType = new Typeface(_font,
				FontStyles.Normal, FontWeights.Normal, FontStretches.Condensed);

			_hasAllRequired = _required.All(x => File.Exists(Path.Combine(_themeDir, x.Value.FileName)));
			_hasAllOptional = _optional.All(x => File.Exists(Path.Combine(_themeDir, x.Value.FileName)));
		}

		public virtual ImageBrush Build()
		{
			_drawingGroup.Children.Clear();

			if(!_hasAllRequired)
				return new ImageBrush();

			AddCardImage();
			AddFadeOverlay();
			AddGem();
			if(Math.Abs(_card.Count) > 1 || _card.Rarity == Rarity.Legendary)
			{
				AddCountBox();
				AddCountText();
			}
			if(_card.IsCreated)
				AddCreatedIcon();
			if(Math.Abs(_card.Count) <= 1 && _card.Rarity == Rarity.Legendary)
				AddLegendaryIcon();
			AddFrame();
			AddCost();
			AddCardName();
			if(_card.Count <= 0 || _card.Jousted)
				AddDarken();

			return new ImageBrush { ImageSource = new DrawingImage(_drawingGroup) };
		}

		protected virtual void AddCardImage()
		{
			var cardFile = Path.Combine(BarImageDir, _card.Id + ".png");
			if(File.Exists(cardFile))
				AddChild(cardFile, _imageRect);
		}

		protected virtual void AddFadeOverlay()
		{
			AddChild(_required[ThemeElement.FadeOverlay]);
		}

		protected virtual void AddFrame()
		{
			var frame = _required[ThemeElement.DefaultFrame];
			if(_card.HighlightFrame)
			{
				frame = _required[ThemeElement.GoldenFrame];
				_card.IsFrameHighlighted = true;
			}
			else
			{
				_card.IsFrameHighlighted = false;
				if(Config.Instance.RarityCardFrames && _hasAllOptional)
				{
					switch(_card.Rarity)
					{
						case Rarity.Rare:
							frame = _optional[ThemeElement.RareFrame];
							break;

						case Rarity.Epic:
							frame = _optional[ThemeElement.EpicFrame];
							break;

						case Rarity.Legendary:
							frame = _optional[ThemeElement.LegendaryFrame];
							break;

						default:
							frame = _optional[ThemeElement.CommonFrame];
							break;
					}
				}
			}
			AddChild(frame);
		}

		protected virtual void AddGem()
		{
			var gem = _required[ThemeElement.DefaultGem];
			if(Config.Instance.RarityCardGems && _hasAllOptional)
			{
				switch(_card.Rarity)
				{
					case Rarity.Rare:
						gem = _optional[ThemeElement.RareGem];
						break;

					case Rarity.Epic:
						gem = _optional[ThemeElement.EpicGem];
						break;

					case Rarity.Legendary:
						gem = _optional[ThemeElement.LegendaryGem];
						break;

					default:
						gem = _optional[ThemeElement.CommonGem];
						break;
				}
			}
			AddChild(gem);
		}

		protected virtual void AddDarken()
		{
			AddChild(_required[ThemeElement.DarkOverlay]);
			if(_card.HighlightFrame)
				AddChild(_required[ThemeElement.GoldenFrame]);
		}

		protected virtual void AddCountBox()
		{
			var countBox = _required[ThemeElement.DefaultCountBox];
			if(Config.Instance.RarityCardFrames && _hasAllOptional)
			{
				switch(_card.Rarity)
				{
					case Rarity.Rare:
						countBox = _optional[ThemeElement.RareCountBox];
						break;

					case Rarity.Epic:
						countBox = _optional[ThemeElement.EpicCountBox];
						break;

					case Rarity.Legendary:
						countBox = _optional[ThemeElement.LegendaryCountBox];
						break;

					default:
						countBox = _optional[ThemeElement.CommonCountBox];
						break;
				}
			}
			AddChild(countBox);
		}

		protected virtual void AddCountText()
		{
			var count = Math.Abs(_card.Count);
			if(count > 1)
			{
				var countText = count > 9 ? "9" : count.ToString();
				var color = new SolidColorBrush(Color.FromRgb(240, 195, 72));
				AddText(countText, 20, new Rect(198, 4, double.NaN, double.NaN), color);
				if(count > 9)
					AddText("+", 13, new Rect(203, 3, double.NaN, double.NaN), color);
			}
		}

		protected virtual void AddCreatedIcon(int offset = 0)
		{
			var xoff = offset == 0 ? -(int)_boxRect.Width : offset;
			if(Math.Abs(_card.Count) > 1 || _card.Rarity == Rarity.Legendary)
				AddChild(_required[ThemeElement.CreatedIcon],
					_required[ThemeElement.CreatedIcon].Rectangle.Move(xoff, 0));
			else
				AddChild(_required[ThemeElement.CreatedIcon]);
		}

		protected virtual void AddLegendaryIcon()
		{
			AddChild(_required[ThemeElement.LegendaryIcon]);
		}

		protected virtual void AddCost()
		{
			AddText(_card.Cost, 22, new Rect(6, 0, 25, 34), Brushes.White, true);
		}

		protected virtual void AddCardName()
		{
			AddText(_card.LocalizedName, 14, new Rect(38, 8, _frameRect.Width - _boxRect.Width - 38, 34), Brushes.White);
		}

		protected virtual void AddText(object obj, int size, Rect rect, Brush fill, bool centered = false)
		{
			foreach(var d in CardTextImageBuilder.GetOutlinedText(obj.ToString(), size, rect, fill, Brushes.Black, centered: centered))
				_drawingGroup.Children.Add(d);
		}

		protected void AddChild(string uri, Rect rect)
			=> _drawingGroup.Children.Add(new ImageDrawing(new BitmapImage(new Uri(uri, UriKind.Relative)), rect));

		protected void AddChild(ThemeElementInfo element)
		{
			_drawingGroup.Children.Add(new ImageDrawing(new BitmapImage(new Uri(
				Path.Combine(_themeDir, element.FileName), UriKind.Relative)), element.Rectangle));
		}

		protected void AddChild(ThemeElementInfo element, Rect overwrite)
		{
			_drawingGroup.Children.Add(new ImageDrawing(new BitmapImage(new Uri(
				Path.Combine(_themeDir, element.FileName), UriKind.Relative)), overwrite));
		}
	}
}