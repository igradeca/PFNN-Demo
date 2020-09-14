DaveDub addition: Added simple FBX character. Character is driven by (global) rotation values coming out of PFNN. 
Todo: 
    Convert rotations to local rotations
    Apply local rotations to avatar for easy transfer to arbitory FBX character (i.e. use Unity's Mechanim system)

# Unity Demonstration for Phase-Function Neural Network for Character Control

Demonstration made in Unity for NN created for character control animations.

The video of demonstration can be seen on following link: https://www.youtube.com/watch?v=mBHTCtaOzzc

Goal of this project is to recreate novel type of functional Neural Network (NN) with Phase-Function which will be able to learn from its dataset and then use it in practical solution. Reason for building the NN is to teach it to create animations of walking, running, jumping, crouching and climbing movements. Also these animations could produce combinations and adaptations to each other according to player inputs in real time. Main advantage of this NN will be that it is useable in real time and it will consume small amount of computer RAM memory.

To play this demonstration user needs to have Xbox360 controller. Character can walk and jog depending on left stick direction amount and with combination of right trigger to jog. Also if user has pressed button B character can crouch. If left trigger button is pressed character facing direction will be the same as camera direction and movement direction is controlled with left stick.

To run this project you need weights for NN computation which can be downloaded on following link:
https://drive.google.com/drive/u/0/folders/1U5eNoEIAEcnsuVD-vNVgYDKIw_87tY5s

Default weights location set in project is in "D:\Network weights\".
To change folder go to "PFNN_CPU.cs" script and change value of variable "WeightsFolderPath" on line 54.
Weights are made by Holden D. in his work Phase-Functioned Neural Networks for Character Control (Holden D., Komura T., Saito J.) by which this whole project is made by.
