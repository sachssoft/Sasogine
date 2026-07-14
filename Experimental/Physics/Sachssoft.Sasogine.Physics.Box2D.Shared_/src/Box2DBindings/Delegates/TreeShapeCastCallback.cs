namespace Box2D;

/// <summary>
/// This function receives clipped ray cast input for a proxy. The function
/// returns the new ray fraction.<br/>
/// - return a value of 0 to terminate the ray cast<br/>
/// - return a value less than input->maxFraction to clip the ray<br/>
/// - return a value of input->maxFraction to continue the ray cast without clipping
/// </summary>
/// <returns>Ray cast input</returns>
public delegate float TreeShapeCastCallback<in TContext>(in ShapeCastInput input, int proxyId, uint64_t userData, TContext context) where TContext : class;

/// <summary>
/// This function receives clipped ray cast input for a proxy. The function
/// returns the new ray fraction.<br/>
/// - return a value of 0 to terminate the ray cast<br/>
/// - return a value less than input->maxFraction to clip the ray<br/>
/// - return a value of input->maxFraction to continue the ray cast without clipping
/// </summary>
/// <returns>Ray cast input</returns>
public delegate float TreeShapeCastRefCallback<TContext>(in ShapeCastInput input, int proxyId, uint64_t userData, ref TContext context) where TContext : unmanaged;

/// <summary>
/// This function receives clipped ray cast input for a proxy. The function
/// returns the new ray fraction.<br/>
/// - return a value of 0 to terminate the ray cast<br/>
/// - return a value less than input->maxFraction to clip the ray<br/>
/// - return a value of input->maxFraction to continue the ray cast without clipping
/// </summary>
/// <returns>Ray cast input</returns>
public delegate float TreeShapeCastCallback(in ShapeCastInput input, int proxyId, uint64_t userData);

/// <summary>
/// This function receives clipped ray cast input for a proxy. The function
/// returns the new ray fraction.<br/>
/// - return a value of 0 to terminate the ray cast<br/>
/// - return a value less than input->maxFraction to clip the ray<br/>
/// - return a value of input->maxFraction to continue the ray cast without clipping
/// </summary>
/// <returns>Ray cast input</returns>
public delegate float TreeShapeCastNintCallback(in ShapeCastInput input, int proxyId, uint64_t userData, nint context);
