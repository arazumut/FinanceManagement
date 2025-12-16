(function (window, document) {
    const Viewer = {
        init(options) {
            if (!options || !options.fileUrl) {
                console.warn('LaaSPdfViewer: fileUrl parametresi zorunludur.');
                return;
            }

            if (typeof pdfjsLib === 'undefined') {
                console.error('pdfjsLib yüklenemedi. CDN bağlantısını kontrol et.');
                return;
            }

            this.fileUrl = options.fileUrl;
            this.canvas = document.getElementById(options.canvasId || 'pdf-canvas');
            if (!this.canvas) {
                console.error('LaaSPdfViewer: canvas bulunamadı.');
                return;
            }

            this.ctx = this.canvas.getContext('2d');
            this.pageNum = 1;
            this.scale = options.initialScale || 1.1;
            this.minScale = options.minScale || 0.6;
            this.maxScale = options.maxScale || 2.5;
            this.pageRendering = false;
            this.pageNumPending = null;

            this.currentPageEl = document.querySelector(options.currentPageSelector);
            this.totalPagesEl = document.querySelector(options.totalPagesSelector);
            this.states = {
                loading: document.querySelector("[data-pdf-state='loading']"),
                error: document.querySelector("[data-pdf-state='error']"),
            };

            const workerSrc =
                options.workerSrc ||
                'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/3.11.174/pdf.worker.min.js';
            pdfjsLib.GlobalWorkerOptions.workerSrc = workerSrc;

            this.bindControls(options.controls || {});
            this.loadDocument();
        },

        bindControls(controls) {
            const prevBtn = document.querySelector(controls.prev);
            const nextBtn = document.querySelector(controls.next);
            const zoomInBtn = document.querySelector(controls.zoomIn);
            const zoomOutBtn = document.querySelector(controls.zoomOut);

            prevBtn &&
                prevBtn.addEventListener('click', (event) => {
                    event.preventDefault();
                    if (this.pageNum <= 1) {
                        return;
                    }
                    this.queueRenderPage(this.pageNum - 1);
                });

            nextBtn &&
                nextBtn.addEventListener('click', (event) => {
                    event.preventDefault();
                    if (!this.pdfDoc || this.pageNum >= this.pdfDoc.numPages) {
                        return;
                    }
                    this.queueRenderPage(this.pageNum + 1);
                });

            zoomInBtn &&
                zoomInBtn.addEventListener('click', (event) => {
                    event.preventDefault();
                    if (this.scale >= this.maxScale) {
                        return;
                    }
                    this.scale = Math.min(this.scale + 0.2, this.maxScale);
                    this.renderPage(this.pageNum);
                });

            zoomOutBtn &&
                zoomOutBtn.addEventListener('click', (event) => {
                    event.preventDefault();
                    if (this.scale <= this.minScale) {
                        return;
                    }
                    this.scale = Math.max(this.scale - 0.2, this.minScale);
                    this.renderPage(this.pageNum);
                });
        },

        loadDocument() {
            this.showState('loading');
            pdfjsLib
                .getDocument(this.fileUrl)
                .promise.then((pdfDoc_) => {
                    this.pdfDoc = pdfDoc_;
                    this.totalPagesEl && (this.totalPagesEl.textContent = this.pdfDoc.numPages);
                    this.renderPage(this.pageNum);
                })
                .catch((error) => {
                    console.error('PDF yüklenirken hata oluştu', error);
                    this.showState('error');
                });
        },

        renderPage(num) {
            this.pageRendering = true;
            this.pdfDoc.getPage(num).then((page) => {
                const viewport = page.getViewport({ scale: this.scale });
                this.canvas.height = viewport.height;
                this.canvas.width = viewport.width;

                const renderContext = {
                    canvasContext: this.ctx,
                    viewport: viewport,
                };
                const renderTask = page.render(renderContext);

                renderTask.promise.then(() => {
                    this.pageRendering = false;
                    this.pageNum = num;
                    this.currentPageEl && (this.currentPageEl.textContent = num);
                    this.showState('ready');

                    if (this.pageNumPending !== null) {
                        const pendingPage = this.pageNumPending;
                        this.pageNumPending = null;
                        this.renderPage(pendingPage);
                    }
                });
            });
        },

        queueRenderPage(num) {
            if (this.pageRendering) {
                this.pageNumPending = num;
            } else {
                this.renderPage(num);
            }
        },

        showState(state) {
            if (state === 'loading') {
                this.toggleState(this.states.loading, true);
                this.toggleState(this.states.error, false);
                this.canvas.style.display = 'none';
            } else if (state === 'error') {
                this.toggleState(this.states.loading, false);
                this.toggleState(this.states.error, true);
                this.canvas.style.display = 'none';
            } else {
                this.toggleState(this.states.loading, false);
                this.toggleState(this.states.error, false);
                this.canvas.style.display = 'block';
            }
        },

        toggleState(element, show) {
            if (!element) {
                return;
            }
            element.classList[show ? 'remove' : 'add']('d-none');
        },
    };

    window.LaaSPdfViewer = Viewer;
})(window, document);

