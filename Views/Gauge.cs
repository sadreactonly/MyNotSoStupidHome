using System;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Graphics;
using Android.Text;
using Android.Content.Res;

namespace MyNotSoStupidHome
{
	public class Gauge : View
	{

        private Paint needlePaint;
        private Path needlePath;
        private Paint needleScrewPaint;

        private float canvasCenterX;
        private float canvasCenterY;
        private float canvasWidth;
        private float canvasHeight;
        private float needleTailLength;
        private float needleWidth;
        private float needleLength;
        private RectF rimRect;
        private Paint rimPaint;
        private Paint rimCirclePaint;
        private RectF faceRect;
        private Paint facePaint;
        private Paint rimShadowPaint;
        private Paint scalePaint;
        private RectF scaleRect;

        private static int totalNicks = 120; // on a full circle
        private float degreesPerNick = totalNicks / 360;
        private float valuePerNick = 10;
        private float minValue = 0;
        private float maxValue = 1000;
        private bool intScale = true;

        private float requestedLabelTextSize = 0;

        private float initialValue = 0;
        private float value = 0;
        private float needleValue = 0;

        private float needleStep;

        private float centerValue;
        private float labelRadius;

        private int majorNickInterval = 10;

        private int deltaTimeInterval = 5;
        private float needleStepFactor = 3f;

        private Paint labelPaint;
        private long lastMoveTime;
        private bool needleShadow = true;
        private int faceColor;

        private Paint upperTextPaint;
        private Paint lowerTextPaint;

        private float requestedTextSize = 0;
        private float requestedUpperTextSize = 0;
        private float requestedLowerTextSize = 0;
        private string upperText = "";
        private string lowerText = "";

        private float textScaleFactor;

        private static readonly int REF_MAX_PORTRAIT_CANVAS_SIZE = 1080; // reference size, scale text accordingly

        public Gauge(Context context): base(context)
        {
            SetLayerType(LayerType.Software, null);
            InitValues();
            InitPaint();
        }
        public Gauge(Context context, IAttributeSet attrs) :
			base(context, attrs)
		{
            SetLayerType(LayerType.Software, null);

            ApplyAttrs(context, attrs);
            InitValues();
            InitPaint();
        }

		public Gauge(Context context, IAttributeSet attrs, int defStyle) :
			base(context, attrs, defStyle)
		{
            SetLayerType(LayerType.Software, null);

            ApplyAttrs(context, attrs);
            InitValues();
            InitPaint();
        }


		
        private void ApplyAttrs(Context context, IAttributeSet attrs)
        {
            TypedArray a = context.ObtainStyledAttributes(attrs, Resource.Styleable.Gauge, 0, 0);
            totalNicks = a.GetInt(Resource.Styleable.Gauge_totalNicks, totalNicks);
            degreesPerNick = 360.0f / totalNicks;
            valuePerNick = a.GetFloat(Resource.Styleable.Gauge_valuePerNick, valuePerNick);
            majorNickInterval = a.GetInt(Resource.Styleable.Gauge_majorNickInterval, 10);
            minValue = a.GetFloat(Resource.Styleable.Gauge_minValue, minValue);
            maxValue = a.GetFloat(Resource.Styleable.Gauge_maxValue, maxValue);
            intScale = a.GetBoolean(Resource.Styleable.Gauge_intScale, intScale);
            initialValue = a.GetFloat(Resource.Styleable.Gauge_initialValue, initialValue);
            requestedLabelTextSize = a.GetFloat(Resource.Styleable.Gauge_labelTextSize, requestedLabelTextSize);
            faceColor = a.GetColor(Resource.Styleable.Gauge_faceColor, Color.Argb(0xff, 0xff, 0xff, 0xff));
            needleShadow = a.GetBoolean(Resource.Styleable.Gauge_needleShadow, needleShadow);
            requestedTextSize = a.GetFloat(Resource.Styleable.Gauge_textSize, requestedTextSize);
            upperText = a.GetString(Resource.Styleable.Gauge_upperText) == null ? upperText : FromHtml(a.GetString(Resource.Styleable.Gauge_upperText)).ToString();
            lowerText = a.GetString(Resource.Styleable.Gauge_lowerText) == null ? lowerText : FromHtml(a.GetString(Resource.Styleable.Gauge_lowerText)).ToString();
            requestedUpperTextSize = a.GetFloat(Resource.Styleable.Gauge_upperTextSize, 0);
            requestedLowerTextSize = a.GetFloat(Resource.Styleable.Gauge_lowerTextSize, 0);
            a.Recycle();

        }

        private void InitValues()
        {
            needleStep = needleStepFactor * ValuePerDegree();
            centerValue = (minValue + maxValue) / 2;
            needleValue = value = initialValue;

            int widthPixels = Resources.DisplayMetrics.WidthPixels;
            textScaleFactor = (float)widthPixels / (float)REF_MAX_PORTRAIT_CANVAS_SIZE;

        }

        private void InitPaint()
        {

            SaveEnabled = true;
            rimPaint = new Paint(PaintFlags.AntiAlias);

            rimCirclePaint = new Paint
            {
                AntiAlias = true
            };
            rimCirclePaint.SetStyle(Paint.Style.Stroke);
            rimCirclePaint.Color = Color.Argb(0x4f, 0x33, 0x36, 0x33);
            rimCirclePaint.StrokeWidth = 0.005f;

            facePaint = new Paint
            {
                AntiAlias = true
            };
            facePaint.SetStyle(Paint.Style.Fill);
            facePaint.Color = new Color(faceColor);

            rimShadowPaint = new Paint();
            rimShadowPaint.SetStyle(Paint.Style.Fill);

            scalePaint = new Paint();
            scalePaint.SetStyle(Paint.Style.Stroke);

            scalePaint.AntiAlias=(true);
            scalePaint.SetARGB(255, 255, 255, 255);



            labelPaint = new Paint
            {
                AntiAlias = true
            };
            labelPaint.SetARGB(255, 255, 255, 255);
            labelPaint.SetTypeface(Typeface.SansSerif);
            labelPaint.TextAlign = (Paint.Align.Center);

            upperTextPaint = new Paint();
            upperTextPaint.SetTypeface(Typeface.SansSerif);
            upperTextPaint.TextAlign = (Paint.Align.Center);
            upperTextPaint.SetARGB(255, 255, 255, 255);


            lowerTextPaint = new Paint();
            lowerTextPaint.SetTypeface(Typeface.SansSerif);
            lowerTextPaint.TextAlign=(Paint.Align.Center);
            lowerTextPaint.SetARGB(255, 255, 255, 255);


            needlePaint = new Paint
            {
                Color = Color.Red
            };
            needlePaint.SetStyle(Paint.Style.FillAndStroke);
            needlePaint.AntiAlias = true;

            needlePath = new Path();

            needleScrewPaint = new Paint
            {
                Color = Color.Black,
                AntiAlias = true
            };
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);

            DrawRim(canvas); 
            DrawFace(canvas); 
            DrawScale(canvas);
            DrawLabels(canvas);
            DrawTexts(canvas);
            canvas.Rotate(ScaleToCanvasDegrees(ValueToDegrees(needleValue)), canvasCenterX, canvasCenterY);
            canvas.DrawPath(needlePath, needlePaint);
            canvas.DrawCircle(canvasCenterX, canvasCenterY, canvasWidth / 61f, needleScrewPaint);

          

            if (NeedsToMove())
            {
                MoveNeedle();
            }
        }

        private void MoveNeedle()
        {
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long deltaTime = currentTime - lastMoveTime;

            if (deltaTime >= deltaTimeInterval)
            {
                if (Math.Abs(value - needleValue) <= needleStep)
                {
                    needleValue = value;
                }
                else
                {
                    if (value > needleValue)
                    {
                        needleValue += 2 * ValuePerDegree();
                    }
                    else
                    {
                        needleValue -= 2 * ValuePerDegree();
                    }
                }
                lastMoveTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                PostInvalidateDelayed(deltaTimeInterval);
            }
        }

        private void DrawRim(Canvas canvas)
        {
            canvas.DrawOval(rimRect, rimPaint);
            canvas.DrawOval(rimRect, rimCirclePaint);
        }

        private void DrawFace(Canvas canvas)
        {
            canvas.DrawOval(faceRect, facePaint);
            canvas.DrawOval(faceRect, rimCirclePaint);
            canvas.DrawOval(faceRect, rimShadowPaint);
        }

        private void DrawScale(Canvas canvas)
        {
            
            canvas.Save();
            for (int i = 0; i < totalNicks; ++i)
            {
                float y1 = scaleRect.Top;
                float y2 = y1 + (0.020f * canvasHeight);
                float y3 = y1 + (0.060f * canvasHeight);
                float y4 = y1 + (0.030f * canvasHeight);

                float value = NickToValue(i);

                if (value >= minValue && value <= maxValue)
                {
                    canvas.DrawLine(0.5f * canvasWidth, y1, 0.5f * canvasWidth, y2, scalePaint);

                    if (i % majorNickInterval == 0)
                    {
                        canvas.DrawLine(0.5f * canvasWidth, y1, 0.5f * canvasWidth, y3, scalePaint);
                    }

                    if (i % (majorNickInterval / 2) == 0)
                    {
                        canvas.DrawLine(0.5f * canvasWidth, y1, 0.5f * canvasWidth, y4, scalePaint);
                    }
                }

                canvas.Rotate(degreesPerNick, 0.5f * canvasWidth, 0.5f * canvasHeight);

            }
            //canvas.Save();
            canvas.Restore();
        }

        private void DrawLabels(Canvas canvas)
        {
            for (int i = 0; i < totalNicks; i += majorNickInterval)
            {
                float value = NickToValue(i);
                if (value >= minValue && value <= maxValue)
                {
                    float scaleAngle = i * degreesPerNick;
                    float scaleAngleRads = ToRadians(scaleAngle);
                    //Log.d(TAG, "i = " + i + ", angle = " + scaleAngle + ", value = " + value);
                    float deltaX = labelRadius * (float)Math.Sin(scaleAngleRads);
                    float deltaY = labelRadius * (float)Math.Cos(scaleAngleRads);
                    string valueLabel;
                    if (intScale)
                    {
                        valueLabel = ((int)value).ToString();
                    }
                    else
                    {
                        valueLabel = value.ToString();
                    }
                    DrawTextCentered(valueLabel, canvasCenterX + deltaX, canvasCenterY - deltaY, labelPaint, canvas);
                }
            }
        }
        private float ToRadians(float scaleAngle)
		{
            return (float)(Math.PI / 180) * scaleAngle;
        }
        private void DrawTexts(Canvas canvas)
        {
            DrawTextCentered(upperText, canvasCenterX, canvasCenterY - (canvasHeight / 6.5f), upperTextPaint, canvas);
            DrawTextCentered(lowerText, canvasCenterX, canvasCenterY + (canvasHeight / 6.5f), lowerTextPaint, canvas);
        }

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            canvasWidth = (float)w;
            canvasHeight = (float)h;
            canvasCenterX = w / 2f;
            canvasCenterY = h / 2f;
            needleTailLength = canvasWidth / 12f;
            needleWidth = canvasWidth / 98f;
            needleLength = (canvasWidth / 2f) * 0.8f;

            needlePaint.StrokeWidth = (canvasWidth / 197f);

            if (needleShadow)
                needlePaint.SetShadowLayer(canvasWidth / 123f, canvasWidth / 10000f, canvasWidth / 10000f, Color.Gray);

            SetNeedle();

            rimRect = new RectF(canvasWidth * .05f, canvasHeight * .05f, canvasWidth * 0.95f, canvasHeight * 0.95f);
            rimPaint.SetShader(new LinearGradient(canvasWidth * 0.40f, canvasHeight * 0.0f, canvasWidth * 0.60f, canvasHeight * 1.0f,
                    Color.Rgb(0xf0, 0xf5, 0xf0),
                    Color.Rgb(0x30, 0x31, 0x30),
                    Shader.TileMode.Clamp));

            float rimSize = 0.02f * canvasWidth;
            faceRect = new RectF();
            faceRect.Set(rimRect.Left + rimSize, rimRect.Top + rimSize,
                    rimRect.Right - rimSize, rimRect.Bottom - rimSize);

            rimShadowPaint.SetShader(new RadialGradient(0.5f * canvasWidth, 0.5f * canvasHeight, faceRect.Width() / 2.0f,
                    new int[] { 0x00000000, 0x00000500, 0x50000500 },
                    new float[] { 0.96f, 0.96f, 0.99f },
                    Shader.TileMode.Mirror));

            scalePaint.StrokeWidth=(0.005f * canvasWidth);
            scalePaint.TextSize=(0.045f * canvasWidth);
            scalePaint.TextScaleX=(0.8f * canvasWidth);

            float scalePosition = 0.015f * canvasWidth;
            scaleRect = new RectF();
            scaleRect.Set(faceRect.Left + scalePosition, faceRect.Top + scalePosition,
                    faceRect.Right - scalePosition, faceRect.Bottom - scalePosition);

            labelRadius = (canvasCenterX - scaleRect.Left) * 0.70f;

            /*
            Log.d(TAG, "width = " + w);
            Log.d(TAG, "height = " + h);
            Log.d(TAG, "width pixels = " + getResources().getDisplayMetrics().widthPixels);
            Log.d(TAG, "height pixels = " + getResources().getDisplayMetrics().heightPixels);
            Log.d(TAG, "density = " + getResources().getDisplayMetrics().density);
            Log.d(TAG, "density dpi = " + getResources().getDisplayMetrics().densityDpi);
            Log.d(TAG, "scaled density = " + getResources().getDisplayMetrics().scaledDensity);
            */

            float textSize;

            if (requestedLabelTextSize > 0)
            {
                textSize = requestedLabelTextSize * textScaleFactor;
            }
            else
            {
                textSize = canvasWidth / 16f;
            }
            //Log.d(TAG, "Label text size = " + textSize);
            labelPaint.TextSize = (textSize);

            if (requestedTextSize > 0)
            {
                textSize = requestedTextSize * textScaleFactor;
            }
            else
            {
                textSize = canvasWidth / 14f;
            }
            //Log.d(TAG, "Default upper/lower text size = " + textSize);
            upperTextPaint.TextSize=(requestedUpperTextSize > 0 ? requestedUpperTextSize * textScaleFactor : textSize);
            lowerTextPaint.TextSize=(requestedLowerTextSize > 0 ? requestedLowerTextSize * textScaleFactor : textSize);

            base.OnSizeChanged(w, h, oldw, oldh);
        }

        private void SetNeedle()
        {
            needlePath.Reset();
            needlePath.MoveTo(canvasCenterX - needleTailLength, canvasCenterY);
            needlePath.LineTo(canvasCenterX, canvasCenterY - (needleWidth / 2));
            needlePath.LineTo(canvasCenterX + needleLength, canvasCenterY);
            needlePath.LineTo(canvasCenterX, canvasCenterY + (needleWidth / 2));
            needlePath.LineTo(canvasCenterX - needleTailLength, canvasCenterY);
            needlePath.AddCircle(canvasCenterX, canvasCenterY, canvasWidth / 49f, Path.Direction.Cw);
            needlePath.Close();

            needleScrewPaint.SetShader(new RadialGradient(canvasCenterX, canvasCenterY, needleWidth / 2,
                    Color.DarkGray, Color.Black, Shader.TileMode.Clamp));
        }

        
    protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);

            int size;
            int width = MeasuredWidth;
            int height = MeasuredHeight;
            int widthWithoutPadding = width - PaddingLeft - PaddingRight;
            int heightWithoutPadding = height - PaddingTop - PaddingBottom;

            if (widthWithoutPadding > heightWithoutPadding)
            {
                size = heightWithoutPadding;
            }
            else
            {
                size = widthWithoutPadding;
            }
            
            SetMeasuredDimension(size + PaddingLeft + PaddingRight, size + PaddingTop + PaddingBottom);
        }

    protected override IParcelable OnSaveInstanceState()
        { 
            Bundle bundle = new Bundle();
            bundle.PutParcelable("superState", base.OnSaveInstanceState());
            bundle.PutFloat("value", value);
            bundle.PutFloat("needleValue", needleValue);
            return bundle;
        }

      
    protected override void OnRestoreInstanceState(IParcelable state)
        {
            if (state is Bundle bundle)
            {
                value = bundle.GetFloat("value");
                needleValue = bundle.GetFloat("needleValue");
                base.OnRestoreInstanceState((IParcelable)bundle.GetParcelable("superState"));
            }
            else
            {
                base.OnRestoreInstanceState(state);
            }
        }

        private float NickToValue(int nick)
        {
            float rawValue = ((nick < totalNicks / 2) ? nick : (nick - totalNicks)) * valuePerNick;
            return rawValue + centerValue;
        }

        private float ValueToDegrees(float value)
        {
            // these are scale degrees, 0 is on top
            return ((value - centerValue) / valuePerNick) * degreesPerNick;
        }

        private float ValuePerDegree()
        {
            return valuePerNick / degreesPerNick;
        }

        private float ScaleToCanvasDegrees(float degrees)
        {
            return degrees - 90;
        }

        private bool NeedsToMove()
        {
            return Math.Abs(needleValue - value) > 0;
        }

        private void DrawTextCentered(string text, float x, float y, Paint paint, Canvas canvas)
        {

            //float xPos = x - (paint.measureText(text)/2f);
            float yPos = (y - ((paint.Descent() + paint.Ascent()) / 2f));
            canvas.DrawText(text, x, yPos, paint);
        }

      

   
        public float Value
		{
            get => this.value;
            set
            {
                needleValue = this.value = value;
                PostInvalidate();
            }
		}
        public void MoveToValue(float value)
        {
            this.value = value;
            PostInvalidate();
        }

        public string UpperText
        {
            get => this.upperText;
            set
            {
                upperText = value;
                Invalidate();
            }
        }
        public string LowerText
        {
            get => this.lowerText;
            set
            {
                lowerText = value;
                Invalidate();
            }
        }
        
        [Obsolete]
        public void SetRequestedTextSize(float size)
        {
            SetTextSize(size);
        }

     
        public void SetTextSize(float size)
        {
            requestedTextSize = size;
        }

        public float TextSize
        {
            get => this.requestedTextSize;
            set
            {
                requestedTextSize = value;
            }
        }

        public float UpperTextSize
        {
            get => this.requestedUpperTextSize;
            set
            {
                requestedUpperTextSize = value;
            }
        }
        public float LowerTextSize
        {
            get => this.requestedLowerTextSize;
            set
            {
                requestedLowerTextSize = value;
            }
        }

        public void SetDeltaTimeInterval(int interval)
        {
            deltaTimeInterval = interval;
        }

   
        public void SetNeedleStepFactor(float factor)
        {
            needleStepFactor = factor;
        }

        public float MinValue
		{
            get => minValue;
			set
			{
                minValue = value;
                InitValues();

                Invalidate();
            }
		}
        public float MaxValue
        {
            get => maxValue;
            set
            {
                maxValue = value;
                InitValues();

                Invalidate();
            }
        }

        public float InitValue
        {
            get => initialValue;
            set
            {
                initialValue = value;
                InitValues();

                Invalidate();
            }
        }

        public int TotalNicks
        {
            get => totalNicks;
            set
            {
                totalNicks = value;
                degreesPerNick = 360.0f / totalNicks;
                InitValues();

                Invalidate();
            }
        }

        public float ValuePerNick
        {
            get => valuePerNick;
            set
            {
                valuePerNick = value;
                InitValues();

                Invalidate();
            }
        }



        public void SetMajorNickInterval(int interval)
        {
            majorNickInterval = interval;
            Invalidate();
        }

        private static ISpanned FromHtml(string html)
        {
            ISpanned result;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
            {
                result = Html.FromHtml(html, FromHtmlOptions.ModeLegacy);
            }
            else
            {
#pragma warning disable CS0618 // Type or member is obsolete
                result = Html.FromHtml(html);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            return result;
        }

    }
}
