# HOW TO PLAY:

### Requirements:
	- Game is built on at least 2 phones
	- Being outside

### Recommended
	- Both phones support AR Core or AR Kit

### Steps:
1. Go outside.
2. Check that your 2 phones have GPS on with a good internet connection.
3. Open the app on your first phone.

 *PHONE 1:*

4. Press the `Create Match` button.
5. A new menu with 3 buttons appears. The top button shows an auto-generated match name. You may click on it to change the match name if you wish. 
	When that is settled, press the `Create` button to begin creating the match.
6. Keep your camera steady and aimed at the environment while it aligns. Preferably face the camera toward a skyline with many buildings for best results. 
	If you are sent back to the starting screen, a corresponding error will be stated at the bottom of the screen.
7. Your camera should now be aligned and you are now by yourself in a multiplayer room you created. 
8. Notice the text in the top left of the screen is the name of the room you are in. Take note of this as you will now make Phone 2 connect to this room.

 *PHONE 2:*

9. Switch to your second phone, preferably move at least a short distance away from the location of your first phone, and open the app.
10. Press the `Find Match` button.
11. You should see the name of the match that phone 1 is in appear on the list of available rooms. Click this button to join that match.
12. This phone will now also perform a camera alignment as you did with phone 1. 
	Keep your phone steady and aimed at the environment while this process completes.
13. Assuming there were no errors, the two phones should now be connected to the same multiplayer room.
	You should be able to see each other if you look around by moving your phone around.
	Use the map view in the bottom right to help figure out where you are in relation to each other.

 *ANY PHONE:*

14. On the right side of the screen, there is a ‘Place Target’ button. If you press it, a target will be placed in the environment on the ground or buildings based on where your crosshairs are pointing. Placing a target can take some time as it calls the Sturfee server and returns the target in a specific GPS position. This target can be seen by all players.
15. The ‘Shoot’ button allows you to shoot projectiles and destroy targets any player places

___


>NOTES:
>
>- This sample game uses UNET, a free Unity Engine multiplayer service. The player who creates a match serves as the host of the game. If this host player leaves the game while other players are still in it, the match will end for everyone.
>
>- If you are not using AR Core or AR Kit supported phones, your digital self will not update its position in the environment as you move about in the real world. The tracking overall will also be worse.