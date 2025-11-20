<script lang="ts">
	import { onMount } from "svelte";
	import * as THREE from "three";
	import { type Snippet } from "svelte";

	interface Event {
		title: string;
		category: string;
		x: number;
		y: number;
		[key: string]: any;
	}

	let { events = [], children }: { events: Event[]; children: Snippet<[{ event: any }]> } =
		$props();

	let canvas: HTMLCanvasElement | undefined = $state();
	let container: HTMLDivElement | undefined = $state();

	const GLOBE_RADIUS = 10;
	const DOT_COUNT = 60000;
	const DOT_SIZE = 0.04;
	const DOT_COLOR = 0x38b579;
	const VIEW_DISTANCE = 25;

	let renderedEvents = $state(
		events.map((e) => ({ ...e, screenX: 0, screenY: 0, visible: false, zIndex: 0 })),
	);

	onMount(() => {
		if (!canvas || !container) return;

		const scene = new THREE.Scene();
		scene.fog = new THREE.Fog(0x000000, 15, 32);

		const camera = new THREE.PerspectiveCamera(
			45,
			container.clientWidth / container.clientHeight,
			0.1,
			1000
		);
		camera.position.z = VIEW_DISTANCE;
		camera.position.y = 8;
		camera.lookAt(0, -2, 0);

		const renderer = new THREE.WebGLRenderer({
			canvas,
			alpha: true,
			antialias: true,
		});
		renderer.setSize(container.clientWidth, container.clientHeight);
		renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));

		const globeGroup = new THREE.Group();
		globeGroup.position.y = -5;
		scene.add(globeGroup);

		// Load texture and generate dots
		const loader = new THREE.TextureLoader();
		loader.load(
			"https://raw.githubusercontent.com/mrdoob/three.js/dev/examples/textures/planets/earth_atmos_2048.jpg",
			(texture) => {
				const img = texture.image;
				const canvas = document.createElement("canvas");
				const ctx = canvas.getContext("2d");
				if (!ctx) return;

				canvas.width = img.width;
				canvas.height = img.height;
				ctx.drawImage(img, 0, 0);
				const imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
				const data = imageData.data;

				const geometry = new THREE.CircleGeometry(DOT_SIZE, 5);
				const material = new THREE.MeshBasicMaterial({
					color: DOT_COLOR,
					side: THREE.DoubleSide,
					transparent: true,
				});

				const dummy = new THREE.Object3D();
				const positions: THREE.Vector3[] = [];

				// Fibonacci sphere algorithm
				const phi = Math.PI * (3 - Math.sqrt(5));

				for (let i = 0; i < DOT_COUNT; i++) {
					const y = 1 - (i / (DOT_COUNT - 1)) * 2;
					const radius = Math.sqrt(1 - y * y);
					const theta = phi * i;

					const x = Math.cos(theta) * radius;
					const z = Math.sin(theta) * radius;

					// Convert to spherical coordinates for texture mapping
					const vector = new THREE.Vector3(x, y, z);
					const spherical = new THREE.Spherical().setFromVector3(vector);

					// Map to UV
					// u: 0 to 1 (longitude)
					// v: 0 to 1 (latitude)
					const u = (spherical.theta + Math.PI) / (2 * Math.PI);
					const v = 1 - (spherical.phi / Math.PI); // Invert v to match texture

					// Sample texture
					const px = Math.floor(u * canvas.width) % canvas.width;
					const py = Math.floor((1 - v) * canvas.height) % canvas.height;
					const index = (py * canvas.width + px) * 4;

					// Simple brightness check or alpha check
					// The reference uses a specific logic: W - H < d
					// F=c[k]/255, _=c[k+1]/255, W=c[k+2]/255, H=(F+_)/2;
					// return W-H<d
					// R, G, B. W is Blue. F is Red, _ is Green.
					// Blue - (Red + Green)/2 < threshold
					// This detects "not water" assuming water is blue.

					const r = data[index];
					const g = data[index + 1];
					const b = data[index + 2];

					// Is land check (approximate from reference)
					// Reference: W - H < 0.1
					// W = b/255, H = (r/255 + g/255)/2
					const isLand = (b/255) - ((r/255 + g/255)/2) < 0.1;

					if (isLand) {
						positions.push(vector.multiplyScalar(GLOBE_RADIUS));
					}
				}

				const instancedMesh = new THREE.InstancedMesh(geometry, material, positions.length);

				positions.forEach((pos, i) => {
					dummy.position.copy(pos);
					dummy.lookAt(0, 0, 0);
					dummy.updateMatrix();
					instancedMesh.setMatrixAt(i, dummy.matrix);
				});

				instancedMesh.instanceMatrix.needsUpdate = true;
				globeGroup.add(instancedMesh);
			}
		);

		// Animation Loop
		let animationFrameId: number;

		const animate = () => {
			animationFrameId = requestAnimationFrame(animate);

			globeGroup.rotation.y += 0.001;

			renderer.render(scene, camera);

			// Update events positions
			const newRenderedEvents = events.map(event => {
				// Map 0-100 to spherical coordinates (matching original logic but scaled)
				const lat = ((event.y - 50) / 50) * -Math.PI * 0.8;
				const lon = ((event.x - 50) / 50) * Math.PI * 2;

				const r = GLOBE_RADIUS;
				const y = Math.sin(lat) * r;
				const rXZ = Math.cos(lat) * r;
				const x = Math.sin(lon) * rXZ;
				const z = Math.cos(lon) * rXZ;

				const pos = new THREE.Vector3(x, y, z);

				pos.applyAxisAngle(new THREE.Vector3(0, 1, 0), globeGroup.rotation.y);

				pos.add(globeGroup.position);

				pos.project(camera);

				const widthHalf = container!.clientWidth / 2;
				const heightHalf = container!.clientHeight / 2;

				const screenX = (pos.x * widthHalf) + widthHalf;
				const screenY = -(pos.y * heightHalf) + heightHalf;

				// Raycast check or simple normal check
				// Since it's a sphere at 0,0,0, and camera is at 0,5,25.
				// We can check the angle between the normal at the point and the vector to camera.

				// Re-calculate world position
				const worldPos = new THREE.Vector3(x, y, z)
					.applyAxisAngle(new THREE.Vector3(0, 1, 0), globeGroup.rotation.y)
					.add(globeGroup.position);
				const cameraPos = new THREE.Vector3();
				camera.getWorldPosition(cameraPos);

				// Check if point is visible
				// Dot product of normal (which is just normalized position for a sphere) and view vector
				const center = globeGroup.position;
				const normal = worldPos.clone().sub(center).normalize();
				const viewVector = cameraPos.clone().sub(worldPos).normalize();
				const dot = normal.dot(viewVector);

				const visible = dot > 0.1; // Slight offset to hide horizon artifacts

				return {
					...event,
					screenX,
					screenY,
					visible,
					zIndex: Math.floor((1 - pos.z) * 1000) // Sort by depth
				};
			});

			renderedEvents = newRenderedEvents;
		};

		animate();

		const handleResize = () => {
			if (!container) return;
			const width = container.clientWidth;
			const height = container.clientHeight;

			camera.aspect = width / height;
			camera.updateProjectionMatrix();
			renderer.setSize(width, height);
		};

		window.addEventListener("resize", handleResize);

		return () => {
			window.removeEventListener("resize", handleResize);
			cancelAnimationFrame(animationFrameId);
			renderer.dispose();
			// Clean up scene...
		};
	});
</script>

<div bind:this={container} class="relative h-full w-full">
	<canvas bind:this={canvas} class="absolute inset-0 h-full w-full"></canvas>

	{#each renderedEvents as event}
		<div
			class="absolute left-0 top-0 will-change-transform"
			style="transform: translate3d({event.screenX}px, {event.screenY}px, 0); opacity: {event.visible
				? 1
				: 0}; z-index: {event.zIndex}; pointer-events: {event.visible ? 'auto' : 'none'};"
		>
			{@render children({ event })}
		</div>
	{/each}
</div>
