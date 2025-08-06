# Interactive Maker
This is a tool for creating interactive media such as movies and shows!

![Screenshot 2025-03-21 145223](https://github.com/user-attachments/assets/61caca1d-aa8a-4a6d-a284-beca7121ade4)

# How to Use:

To start, go to File > Open Folder and select a new project folder, or an existing interactive folder

![image](https://github.com/user-attachments/assets/46837ab7-0326-4d7c-907a-34a5c81b3dac)

Have an MP4 or MKV video file, if it's not one of these types, then I suggest to convert them. If possible, convert to 48 kHz for smoother transitions and better audio quailty.
Worst case, you could just rename the file extension

* Create a Viewable ID, it can be anything other than empty, but I recommend it to be an 8 digit number

* Set the Initial Segment to a segment in the project, this will be the first segment played

![image](https://github.com/user-attachments/assets/f28be4dc-9bca-437e-bfb5-aedf36868d5c)

* You can create a new segment with the "Create New Segment" button

* If you want to edit a segment, double click it in the Workspace or List View

* When creating/editing a segment, you can change the name of it, and change the "Default Next" which is the segment that will play after the segment finishes, if no choices exist

* The first Set Start and Set End are the start and end timestamps in the video file that the segment is in

* The next Set Start and Set End are the start and end timestamps in the video file that the choice will show up in the segment

* You can use the "Copy" button to copy choices from another segment

* Lastly, when done creating or editing a segment you can hit "Save" to confirm the creation or edits of a segment

![image](https://github.com/user-attachments/assets/3075653d-d6a3-4a40-acec-ad111664bb2b)

* You can hit "Add Choice" to create a new choice

* Change the Segment to go to when the choice is made with the "Segment" drop down

* Change the text that the choice displays in "Choice Text"

* Hit OK to confirm that choice

* If you've created variables, you can select a variable and what it needs to equal, then hit "Add Variable" to make it so that when that choice is selected, then that variable is changed to that value

* If you want to remove variables changed by the choice, select the variable and hit the "Remove" button

![image](https://github.com/user-attachments/assets/fcde52d5-0560-4725-bec9-5ab9a9516f3c)

* When setting the Start and End times of a segment or choice point, it'll load the video file. In this you can lock in the current time in the video as the timestamp

* You can hit "Play" or "Pause" to pause and unpause the video

* When playing the video, the "Backward" and "Forward" button will skip 10 seconds backwards or forwards! If paused, it moves 1 single millisecond

* The "Last Frame" and "Next Frame" will move you one frame backwards or forwards, or approximately 40 milliseconds

![image](https://github.com/user-attachments/assets/d5b8016a-1665-4ad2-8d1f-d8257abdde60)

* You can manage variables with the "Manage Variables" button or go to Edit > Variables

* To create a variable put in the name of the variable in the "Variable Name" and you can put the default variable value in the "Variable Value"

* When done, hit "Add Variable" to confirm your variable

* You can remove variables by selecting them then hitting "Remove"

* When done creating/editing variables, hit "Save" to save your variables

![image](https://github.com/user-attachments/assets/062a65e8-1859-4494-9d67-408bc48836bc)

* You can manage preconditions with the "Manage Preconditions" button or go to Edit > Preconditions

* You can add the name of the precondition in the "Precondition Name"

* Preconditions are like "if" statements, in this case you'd select a varible and then select what value it needs to equal for the precondition to be met

* You can remove preconditions by selecting them then hitting "Remove"

![image](https://github.com/user-attachments/assets/c0fdb362-1f14-45db-8d1f-39fac438c91a)

* You can manage segment groups with the "Manage Segment Groups" button or go to Edit > Segment Groups

* Segment Groups are a little more complicated than you think. First off, THE SEGMENT GROUP NAME MATTERS!!!!!

* If you name a segment group the same thing as a segment, then at segment's default next is overiden, and instead the segment group acts as an evulation method to see what the next segment will be

* Currently you can't go to a segment group, but in a future update when you're given a dropdown for a segment, you can select a segment group

* The "Default Segment" is a segment that you'll be sent to if all of the preconditions aren't met in the segment group

* Next you add a segment and a precondition, you can then hit "Add", if that precondition is met then you'll be sent to that segment

* You can remove preconditions by selecting them then hitting "Remove"

* Lastly, to confirm a segment group, hit "Add Group"

* You can remove segment groups by selecting them then hitting "Remove"

* When finished creating/editing segment groups hit "Save"

![image](https://github.com/user-attachments/assets/903a78f4-e382-43d1-9743-0146cd9f2964)

* You can hit "Workspace" to view a gird of the interactive

* You can drag around segments and double click them to edit them

* You can drag the grid to pan around

* You can use the scroll wheel to zoom in and out

![Untitled](https://github.com/user-attachments/assets/95afa074-c0b8-428b-b250-ba27c57182ae)

That's about it! When done just hit "Save JSONs" or File > Save JSONs to save your work!
