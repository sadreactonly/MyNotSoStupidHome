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

        private  const string TAG = "";
        private Paint labelPaint;
        private long lastMoveTime;
        private bool needleShadow = true;
        private int faceColor;
        private int scaleColor;
        private int needleColor;
        private Paint upperTextPaint;
        private Paint lowerTextPaint;

        private float requestedTextSize = 0;
        private float requestedUpperTextSize = 0;
        private float requestedLowerTextSize = 0;
        private String upperText = "";
        private String lowerText = "";

        private float textScaleFactor;

        private static int REF_MAX_PORTRAIT_CANVAS_SIZE = 1080; // reference size, scale text accordingly

        public Canvas can;
        public Gauge(Context context): base(context)
        {
            SetLayerType(LayerType.Software, null);
            initValues();
            initPaint();
        }
        public Gauge(Context context, IAttributeSet attrs) :
			base(context, attrs)
		{
            SetLayerType(LayerType.Software, null);

            applyAttrs(context, attrs);
            initValues();
            initPaint();
        }

		public Gauge(Context context, IAttributeSet attrs, int defStyle) :
			base(context, attrs, defStyle)
		{
            SetLayerType(LayerType.Software, null);

            applyAttrs(context, attrs);
            initValues();
            initPaint();
        }


		
        private void applyAttrs(Context context, IAttributeSet attrs)
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
            scaleColor = a.GetColor(Resource.Styleable.Gauge_scaleColor, Color.Blue);
            needleColor = a.GetColor(Resource.Styleable.Gauge_needleColor, Color.Red);
            needleShadow = a.GetBoolean(Resource.Styleable.Gauge_needleShadow, needleShadow);
            requestedTextSize = a.GetFloat(Resource.Styleable.Gauge_textSize, requestedTextSize);
            upperText = a.GetString(Resource.Styleable.Gauge_upperText) == null ? upperText : fromHtml(a.GetString(Resource.Styleable.Gauge_upperText)).ToString();
            lowerText = a.GetString(Resource.Styleable.Gauge_lowerText) == null ? lowerText : fromHtml(a.GetString(Resource.Styleable.Gauge_lowerText)).ToString();
            requestedUpperTextSize = a.GetFloat(Resource.Styleable.Gauge_upperTextSize, 0);
            requestedLowerTextSize = a.GetFloat(Resource.Styleable.Gauge_lowerTextSize, 0);
            a.Recycle();

            validate();
        }

        private void initValues()
        {
            needleStep = needleStepFactor * valuePerDegree();
            centerValue = (minValue + maxValue) / 2;
            needleValue = value = initialValue;

            int widthPixels = Resources.DisplayMetrics.WidthPixels;
            textScaleFactor = (float)widthPixels / (float)REF_MAX_PORTRAIT_CANVAS_SIZE;

        }

        private void initPaint()
        {

            SaveEnabled = true;

            // Rim and shadow are based on the Vintage Thermometer:
            // http://mindtherobot.com/blog/272/android-custom-ui-making-a-vintage-thermometer/

            rimPaint = new Paint(PaintFlags.AntiAlias);

            rimCirclePaint = new Paint();
            rimCirclePaint.AntiAlias = true;
            rimCirclePaint.SetStyle(Paint.Style.Stroke);
            rimCirclePaint.Color = Color.Argb(0x4f, 0x33, 0x36, 0x33);
            rimCirclePaint.StrokeWidth = 0.005f;

            facePaint = new Paint();
            facePaint.AntiAlias = true;
            facePaint.SetStyle(Paint.Style.Fill);
            facePaint.Color = new Color(faceColor);

            rimShadowPaint = new Paint();
            rimShadowPaint.SetStyle(Paint.Style.Fill);

            scalePaint = new Paint();
            scalePaint.SetStyle(Paint.Style.Stroke);

            scalePaint.AntiAlias=(true);
            //scalePaint.Color = new Color(scaleColor);
            scalePaint.SetARGB(255, 255, 255, 255);



            labelPaint = new Paint();
            labelPaint.AntiAlias = true;
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


            needlePaint = new Paint();
            needlePaint.Color = Color.Red;
            needlePaint.SetStyle(Paint.Style.FillAndStroke);
            needlePaint.AntiAlias = true;

            needlePath = new Path();

            needleScrewPaint = new Paint();
            needleScrewPaint.Color= Color.Black;
            needleScrewPaint.AntiAlias = true;
        }

    protected override void OnDraw(Canvas canvas)
        {
            can = canvas;
            base.OnDraw(canvas);

            drawRim(canvas); //radi
            drawFace(canvas); //radi
            drawScale(canvas);
            drawLabels(canvas);
            drawTexts(canvas);
            canvas.Rotate(scaleToCanvasDegrees(valueToDegrees(needleValue)), canvasCenterX, canvasCenterY);
            canvas.DrawPath(needlePath, needlePaint);
            canvas.DrawCircle(canvasCenterX, canvasCenterY, canvasWidth / 61f, needleScrewPaint);

          

            if (needsToMove())
            {
                moveNeedle();
            }
        }

        private void moveNeedle()
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
                        needleValue += 2 * valuePerDegree();
                    }
                    else
                    {
                        needleValue -= 2 * valuePerDegree();
                    }
                }
                lastMoveTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                PostInvalidateDelayed(deltaTimeInterval);
            }
        }

        private void drawRim(Canvas canvas)
        {
            canvas.DrawOval(rimRect, rimPaint);
            canvas.DrawOval(rimRect, rimCirclePaint);
        }

        private void drawFace(Canvas canvas)
        {
            canvas.DrawOval(faceRect, facePaint);
            canvas.DrawOval(faceRect, rimCirclePaint);
            canvas.DrawOval(faceRect, rimShadowPaint);
        }

        private void drawScale(Canvas canvas)
        {
            
            canvas.Save();
            for (int i = 0; i < totalNicks; ++i)
            {
                float y1 = scaleRect.Top;
                float y2 = y1 + (0.020f * canvasHeight);
                float y3 = y1 + (0.060f * canvasHeight);
                float y4 = y1 + (0.030f * canvasHeight);

                float value = nickToValue(i);

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

        private void drawLabels(Canvas canvas)
        {
            for (int i = 0; i < totalNicks; i += majorNickInterval)
            {
                float value = nickToValue(i);
                if (value >= minValue && value <= maxValue)
                {
                    float scaleAngle = i * degreesPerNick;
                    float scaleAngleRads = ToRadians(scaleAngle);
                    //Log.d(TAG, "i = " + i + ", angle = " + scaleAngle + ", value = " + value);
                    float deltaX = labelRadius * (float)Math.Sin(scaleAngleRads);
                    float deltaY = labelRadius * (float)Math.Cos(scaleAngleRads);
                    String valueLabel;
                    if (intScale)
                    {
                        valueLabel = ((int)value).ToString();
                    }
                    else
                    {
                        valueLabel = value.ToString();
                    }
                    drawTextCentered(valueLabel, canvasCenterX + deltaX, canvasCenterY - deltaY, labelPaint, canvas);
                }
            }
        }
        private float ToRadians(float scaleAngle)
		{
            return (float)(Math.PI / 180) * scaleAngle;
        }
        private void drawTexts(Canvas canvas)
        {
            drawTextCentered(upperText, canvasCenterX, canvasCenterY - (canvasHeight / 6.5f), upperTextPaint, canvas);
            drawTextCentered(lowerText, canvasCenterX, canvasCenterY + (canvasHeight / 6.5f), lowerTextPaint, canvas);
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

            setNeedle();

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

        private void setNeedle()
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
            if (state is Bundle) {
                Bundle bundle = (Bundle)state;
                value = bundle.GetFloat("value");
                needleValue = bundle.GetFloat("needleValue");
                base.OnRestoreInstanceState((IParcelable)bundle.GetParcelable("superState"));
            } else
            {
                base.OnRestoreInstanceState(state);
            }
        }

        private float nickToValue(int nick)
        {
            float rawValue = ((nick < totalNicks / 2) ? nick : (nick - totalNicks)) * valuePerNick;
            return rawValue + centerValue;
        }

        private float valueToDegrees(float value)
        {
            // these are scale degrees, 0 is on top
            return ((value - centerValue) / valuePerNick) * degreesPerNick;
        }

        private float valuePerDegree()
        {
            return valuePerNick / degreesPerNick;
        }

        private float scaleToCanvasDegrees(float degrees)
        {
            return degrees - 90;
        }

        private bool needsToMove()
        {
            return Math.Abs(needleValue - value) > 0;
        }

        private void drawTextCentered(String text, float x, float y, Paint paint, Canvas canvas)
        {

            //float xPos = x - (paint.measureText(text)/2f);
            float yPos = (y - ((paint.Descent() + paint.Ascent()) / 2f));
            canvas.DrawText(text, x, yPos, paint);
        }

        /**
         * Set gauge to value.
         *
         * @param value Value
         */
        public void setValue(float value)
        {
            needleValue = this.value = value;
            PostInvalidate();
        }

        /**
         * Animate gauge to value.
         *
         * @param value Value
         */
        public void moveToValue(float value)
        {
            this.value = value;
            PostInvalidate();
        }

        /**
         * Set string to display on upper gauge face.
         *
         * @param text Text
         */
        public void setUpperText(String text)
        {
            upperText = text;
            Invalidate();
        }

        /**
         * Set string to display on lower gauge face.
         *
         * @param text Text
         */
        public void setLowerText(String text)
        {
            lowerText = text;
            Invalidate();
        }

        /**
         * Request a text size.
         *
         * @param size Size (pixels)
         * @see Paint#setTextSize(float);
         */
        
        [Obsolete]
        public void setRequestedTextSize(float size)
        {
            setTextSize(size);
        }

        /**
         * Set a text size for the upper and lower text.
         *
         * Size is in pixels at a screen width (max. canvas width/height) of 1080 and is scaled
         * accordingly at different resolutions. E.g. a value of 48 is unchanged at 1080 x 1920
         * and scaled down to 27 at 600 x 1024.
         *
         * @param size Size (relative pixels)
         * @see Paint#setTextSize(float);
         */
        public void setTextSize(float size)
        {
            requestedTextSize = size;
        }

        /**
         * Set or override the text size for the upper text.
         *
         * Size is in pixels at a screen width (max. canvas width/height) of 1080 and is scaled
         * accordingly at different resolutions. E.g. a value of 48 is unchanged at 1080 x 1920
         * and scaled down to 27 at 600 x 1024.
         *
         * @param size (relative pixels)
         * @see Paint#setTextSize(float);
         */
        public void setUpperTextSize(float size)
        {
            requestedUpperTextSize = size;
        }

        /**
         * Set or override the text size for the lower text
         *
         * Size is in pixels at a screen width (max. canvas width/height) of 1080 and is scaled
         * accordingly at different resolutions. E.g. a value of 48 is unchanged at 1080 x 1920
         * and scaled down to 27 at 600 x 1024.
         *
         * @param size (relative pixels)
         * @see Paint#setTextSize(float);
         */
        public void setLowerTextSize(float size)
        {
            requestedLowerTextSize = size;
        }

        /**
         * Set the delta time between movement steps during needle animation (default: 5 ms).
         *
         * @param interval Time (ms)
         */
        public void setDeltaTimeInterval(int interval)
        {
            deltaTimeInterval = interval;
        }

        /**
         * Set the factor that determines the step size during needle animation (default: 3f).
         * The actual step size is calulated as follows: step_size = step_factor * scale_value_per_degree.
         *
         * @param factor Step factor
         */
        public void setNeedleStepFactor(float factor)
        {
            needleStepFactor = factor;
        }


        /**
         * Set the minimum scale value.
         *
         * @param value minimum value
         */
        public void setMinValue(float value)
        {
            minValue = value;
            initValues();
            validate();
            Invalidate();
        }

        /**
         * Set the maximum scale value.
         *
         * @param value maximum value
         */
        public void setMaxValue(float value)
        {
            maxValue = value;
            initValues();
            validate();
            Invalidate();
        }

        public void setInitValue(float value)
        {
            initialValue = value;
            initValues();
            validate();
            Invalidate();
        }
        /**
         * Set the total amount of nicks on a full 360 degree scale. Should be a multiple of majorNickInterval.
         *
         * @param nicks number of nicks
         */
        public void setTotalNicks(int nicks)
        {
            totalNicks = nicks;
            degreesPerNick = 360.0f / totalNicks;
            initValues();
            validate();
            Invalidate();
        }

        /**
         * Set the value (interval) per nick.
         *
         * @param value value per nick
         */
        public void setValuePerNick(float value)
        {
            valuePerNick = value;
            initValues();
            validate();
            Invalidate();
        }

        /**
         * Set the interval (number of nicks) between enlarged nicks.
         *
         * @param interval major nick interval
         */
        public void setMajorNickInterval(int interval)
        {
            majorNickInterval = interval;
            validate();
            Invalidate();
        }

        private void validate()
        {
            bool valid = true;
            if (totalNicks % majorNickInterval != 0)
            {
                valid = false;
               // Log.w(TAG, getResources().getString(R.string.invalid_number_of_nicks, totalNicks, majorNickInterval));
            }
            float sum = minValue + maxValue;
            int intSum = (int)Math.Round(sum);
            if ((maxValue >= 1 && (sum != intSum || (intSum & 1) != 0)) || minValue >= maxValue)
            {
                valid = false;
               // Log.w(TAG, getResources().getString(R.string.invalid_min_max_ratio, minValue, maxValue));
            }
            if (Math.Round(sum % valuePerNick) != 0)
            {
                valid = false;
               // Log.w(TAG, getResources().getString(R.string.invalid_min_max, minValue, maxValue, valuePerNick));
            }
            //if (valid)// Log.i(TAG, getResources().getString(R.string.scale_ok));
        }

    
        private static ISpanned fromHtml(String html)
        {
            ISpanned result;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
            {
                result = Html.FromHtml(html, FromHtmlOptions.ModeLegacy);
            }
            else
            {
                result = Html.FromHtml(html);
            }
            return result;
        }

    }
}
