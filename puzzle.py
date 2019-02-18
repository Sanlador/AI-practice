#This program implements the RDFS and IDA* to solve a 15 puzzle
import random
import math
import time
import numpy as np
globalA = 0
globalB = 0

compass = ["up","down","left","right"]
direction = {
	"up": (-1, 0),
	"down": (1, 0),
	"right": (0, 1),
	"left": (0, -1)
}

	#create a 4x4 grid with values 0 to 15 randomly assigned (0 is considered the empty space)
def createBoard():
	board = np.zeros((4,4))
	count = 1
	for i in range(4):
		for j in range(4):
			board[i][j] = count
			count += 1
	board[3][3] = 0
	return board

#returns a board position for a given a move
def move(board, dir):
	boardCopy = np.copy(board)
	position = np.where(board == 0)
	#check for illegal moves
	if position[0][0] == 0 and dir == "up":
		return []
	if position[0][0] == 3 and dir == "down":
		return []
	if position[1][0] == 0 and dir == "left":
		return []
	if position[1][0] == 3 and dir == "right":
		return []
	move = [position[0][0] + direction[dir][0], position[1][0] + direction[dir][1]]
	boardCopy[position[0][0]][position[1][0]] = boardCopy[move[0]][move[1]]
	boardCopy[move[0]][move[1]] = 0
	return boardCopy

#checks for sucessful board state
def goalTest(board):
	count = 1
	for i in range(4):
		for j in range(4):
			if i == 3 and j == 3:
				return True
			elif board[i][j] != count:
				return False
			count += 1

#Performs a series of m random moves where m is an inputted integer value
def scramble(board, m):
	compass = ["up", "down", "left", "right"]
	for i in range(m):
		control = False
		while control == False:
			r = random.randint(0,3)
			if move(board, compass[r]) != []:
				board = move(board, compass[r])
				control = True
				#print(board)
	return board

#measures a heuristic of a given board position
def heuristic(board, alt = False):
	h = 0
	for i in range(15):
		col = 0
		row = 0
		loc = np.where(board == i + 1)
		for j in range(i):
			col += 1
			if col == 4:
				col = 0
				row += 1
		distance = abs(loc[0] - row) + abs(loc[1] - col)
		h += distance
	#linear conflict addition
	if alt and distance > 0:
		locRow = loc[0]
		locCol = loc[1]
		for j in range(4):
			if loc[0] != j:
				if board[j][locCol][0] != j * 4 + locCol:
					if board[j][locCol][0] % locCol == 0:
						h += 2
		for k in range(4):
			if loc[0] != k:
				if board[locRow][0][k] != (k * 4 + locRow):
					if board[locRow][0][k] % locRow == 0:
						h += 2
	return h

#initiates an IDA* search
def IDA(root, heu = False):
	bound = heuristic(root)
	path = [root]
	t = ""
	while t != "found":
		#begins a recursive search
		t = IDASearch(path, 0, bound, heu, 0)
		#increases the bound, allowing the search to go deeper
	return t

#performs a recurseive search using a heuristic heuristic
def IDASearch(path, g, bound, heu, count):
	print(count)
	global globalA
	globalA += 1
	root = path[-1]
	f = g + heuristic(root, heu)
	#print(f)
	if f > bound:
		return f
	if goalTest(root):
		return "found"

	#selects legal choices
	choice = []
	min = math.inf
	for i in range(4):
		if move(root, compass[i]) != []:
			choice.append(move(root, compass[i]))
	#print(choice)
	for i in choice:
		control = False
		#checks if configuration has a previously reached state, and culls it from the list of options if so
		for j in range(len(path)):
			if np.array_equal(j,path[j]):
				control = True
		if control == False:

			#print(root)
			path.append(i)
			t = IDASearch(path, g + 1, bound, heu, count + 1)
			if t == "found":
				return "found"
			elif t < min:
				min = t
			del path[-1]
	
	return min

#performs a recursive best first search

def RBFS(board, fLimit, heu = False):
	print(count)
	global globalA
	fn = heuristic(board, heu)
	if goalTest(board):
		return "success", heuristic(board, heu)
	choice = []
	for i in range(5):
		if move(board, compass[i]) != []:
			moveCost = heuristic((move(board, compass[i])), heu)
			choice.append((move(board, compass[i]), max(moveCost,fn)))

	succ = [choice[0]]
	for i in range(1, len(choice)):
		for j in range(len(succ)):
			if (choice[i][1]) <= (succ[j][1]):
				succ.insert(j,choice[i])
			elif j == len(succ) - 1:
				succ.append(choice[i])

	control = True
	while control == True:
		#print(succ)
		#print("***")
		best = []
		best.append(succ[0][0])
		best.append(succ[0][1])
		#print((best))
		alt = succ[1][0]
		if heuristic(best[0], heu) > fLimit:
			return "Fail", best[1]
		result, best[1] = RBFS(best[0], min(fLimit, heuristic(alt, heu)), count + 1)
		if result == "success":
			return result, best[1]
			control = False

b1 = createBoard()			
b1 = scramble(b1, (0 + 5) * 10)
b2 = b1	
globalA = 0
print(RBFS(b1))
print(globalA)
'''
f = open("runtimes.csv", "w")
f.write("IDA-manhattan, IDA-my, RBFS-manhattan, RBFS-my\n")
for i in range(2,3):
	f.write("i = ")
	f.write(str(i + 1))
	f.write("\n")
	for j in range(10):
		#print(str(i))
		#print(str(j))
		b1 = createBoard()
		b1 = scramble(b1, (i + 1) * 10)
		b2 = b1
		b3 = b1
		b4 = b1
		start = time.time()
		print(globalA)
		globalA = 0
		print(IDA(b1))
		print(globalA)
		GlobalA = 0
		print("XXX")
		end = time.time()
		f.write(str(end-start))
		f.write(",")
		start = time.time()
		#print(IDA(b2, True))
		#print("###")
		end = time.time()
		f.write(str(end-start))
		f.write(",")
		start = time.time()
		RBFS(b3, math.inf)
		end = time.time()
		f.write(str(end-start))
		f.write(",")
		start = time.time()
		RBFS(b4, math.inf, True)
		end = time.time()
		f.write(str(end-start))
		f.write("\n")
		
start = time.time()
print(RBFS(b2, math.inf, True))
end = time.time()
print(end - start)
'''