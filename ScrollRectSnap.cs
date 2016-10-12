/*
 * Copyright (c) 2016 Adrien de Sentenac
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Replaces the default ScrollRect to provide snapping to element.
/// Expects all Content children to have the same width or height (if the ScrollView is horizontal or vertical - does not support both).
/// </summary>
public class ScrollRectSnap : ScrollRect {
	public delegate void ElementSnappedDelegate(int index);
	public event ElementSnappedDelegate ElementSnappedEvent;

	private enum SnapTo {
		Next,
		Previous,
		Closest
	}

	private float _nbSteps;
	private float _target;
	private float _origin;
	private float _snapStartTime;
	private float _dragStartTime;

	public override void OnBeginDrag (PointerEventData eventData) {
		base.OnBeginDrag(eventData);
		_target = -1;
		_dragStartTime = Time.time;
	}

	public override void OnEndDrag (PointerEventData eventData) {
		base.OnEndDrag(eventData);
		SnapTo snapTo = SnapTo.Closest;
		if (Time.time - _dragStartTime < 0.2) {
			if (horizontal)
				snapTo = eventData.position.x > eventData.pressPosition.x ? SnapTo.Previous : SnapTo.Next;
			else
				snapTo = eventData.position.y > eventData.pressPosition.y ? SnapTo.Previous : SnapTo.Next;
		}
		SnapToElement(snapTo);
	}

	public override void OnScroll (PointerEventData data) {
		base.OnScroll(data);
		if (horizontal)
			SnapToElement(data.scrollDelta.x < 0 ? SnapTo.Next : SnapTo.Previous);
		else
			SnapToElement(data.scrollDelta.y < 0 ? SnapTo.Next : SnapTo.Previous);
	}

	public void SnapToElement(int index) {
		_origin = horizontal ? this.horizontalScrollbar.value : this.verticalScrollbar.value;
		_snapStartTime = Time.time;
		_target = index / _nbSteps;
	}

	public void JumpToElement(int index) {
		float value = index / _nbSteps;
		Canvas.ForceUpdateCanvases ();
		if (horizontal)
			this.horizontalScrollbar.value = value;
		else
			this.verticalScrollbar.value = value;
		Canvas.ForceUpdateCanvases ();
	}

	protected override void Awake() {
		base.Awake();
	 	_nbSteps = this.content.transform.childCount - 1;
		_target = -1;
	}

	private void Update() {
		if (_target != -1) {
			float progress = Mathf.Min((Time.time - _snapStartTime) * 2.0f, 1.0f);
			float value = Mathf.Lerp(_origin, _target, progress);

			if (progress == 1.0f) {
				if (ElementSnappedEvent != null) {
					ElementSnappedEvent(Mathf.FloorToInt(_target * _nbSteps));
				}
				_target = -1;
				_origin = value;
			}

			Canvas.ForceUpdateCanvases ();
			if (horizontal)
				this.horizontalScrollbar.value = value;
			else
				this.verticalScrollbar.value = value;
			Canvas.ForceUpdateCanvases ();
		}
	}

	private void SnapToElement(SnapTo snapTo) {
		_origin = horizontal ? this.horizontalScrollbar.value : this.verticalScrollbar.value;
		_snapStartTime = Time.time;
		float prev = Mathf.Floor(_origin * _nbSteps) / _nbSteps;
		float next = Mathf.Ceil(_origin * _nbSteps) / _nbSteps;
		switch (snapTo) {
		case SnapTo.Next:
			_target = next;
			break;
		case SnapTo.Previous:
			_target = prev;
			break;
		case SnapTo.Closest:
			if (_origin - prev > next - _origin) {
				_target = next;
			} else {
				_target = prev;
			}
			break;
		}
	}
}
