### Gestures
* Get a reasonable framework up and running with one gesture: *tap*
* Add a **testing mechanism** while adding another gesture: *drag*
* Implement **Gesture Primitives** 
 - double-tap
 - double tap+drag
 - drag (is this a thing? tap>move>release?)
 - swipe (is this different from drag? never stationary?)
 - hold
 - loop
 - double tap loop
 - double loop ?
 - dotted drag (drag while briefly lifting finger in the middle)
 - strike through (this is intended to go through a target)
 - double strike (through a target and back)
### Implementation thoughts
1. Recording gestures for playback through a testing mechanism should be straight forward. Collect the raw input or whatever the gesture controller receives. *You don't need to test both the raw input collector and the gesture controller*
2. Draw points of input on the screen when testing. It will be necessary to see that the input has the correct gesture response. Perhaps color code the input from blue to green to show start and end.

