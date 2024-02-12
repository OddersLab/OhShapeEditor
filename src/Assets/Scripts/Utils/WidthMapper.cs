namespace Utils
{
	public static class WidthMapper
	{
		private const float _WidthForDefaultZoom = 21.06f;
		
		public static float GetAnchor(float time, float zoom)
		{
			float ancho = (_WidthForDefaultZoom * time * zoom) / 10f;
			return ancho;
		}
	}
}
